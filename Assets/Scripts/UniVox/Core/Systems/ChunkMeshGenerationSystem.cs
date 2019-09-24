using System.Collections.Generic;
using Types;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEdits;
using UnityEdits.Hybrid_Renderer;
using UnityEngine.Profiling;
using UniVox.Core.Types;
using Material = UnityEngine.Material;
using MeshCollider = Unity.Physics.MeshCollider;

namespace UniVox.Core.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(RenderMeshSystemV2))]
    [UpdateBefore(typeof(RenderMeshSystemV3))]
    public class ChunkMeshGenerationSystem : JobComponentSystem
    {
        public struct ChunkRenderVersion : ISystemStateComponentData
        {
            public uint Info;
            public uint Render;

            public bool DidChange(Version info, Version render) =>
                ChangeVersionUtility.DidChange(info, Info) || ChangeVersionUtility.DidChange(render, Render);

            public bool DidChange(Chunk chunk) => DidChange(chunk.Info.Version, chunk.Render.Version);

            public static ChunkRenderVersion Create(Chunk chunk)
            {
                return new ChunkRenderVersion()
                {
                    Info = chunk.Info.Version,
                    Render = chunk.Render.Version
                };
            }
        }

        private EntityQuery _renderQuery;
        private EntityQuery _setupQuery;
        private EntityQuery _cleanupQuery;

        private Universe _universe;
//        private Material _material;

        private Dictionary<UniversalChunkId, NativeArray<Entity>> _entityCache;
        private EntityArchetype _archetype;

        private void SetupArchetype()
        {
            _archetype = EntityManager.CreateArchetype(
                //Rendering
                typeof(RenderMesh),
                typeof(LocalToWorld),
//Physics
                typeof(Translation),
                typeof(Rotation),
                typeof(PhysicsCollider));
        }

        protected override void OnCreate()
        {
            FrameCaches = new Queue<FrameCache>();
            _universe = GameManager.Universe;
//            GameManager.MasterRegistry.Material.TryGetValue(0, out _material);
            SetupArchetype();
            _renderQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                    ComponentType.ReadWrite<ChunkRenderVersion>()
                }
            });
            _setupQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                },
                None = new[]
                {
                    ComponentType.ReadWrite<ChunkRenderVersion>()
                }
            });
            _cleanupQuery = GetEntityQuery(new EntityQueryDesc()
            {
                None = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                },
                All = new[]
                {
                    ComponentType.ReadWrite<ChunkRenderVersion>()
                }
            });

            _entityCache = new Dictionary<UniversalChunkId, NativeArray<Entity>>();
        }

        protected override void OnDestroy()
        {
            //TODO also destory Entities
            _entityCache.Dispose();
            _entityCache.Clear();
        }


        NativeArray<Entity> GetResizedCache(UniversalChunkId id, int desiredLength)
        {
            ResizeCache(id, desiredLength);
            return _entityCache[id];
        }

        void ResizeCache(UniversalChunkId id, int desiredLength)
        {
            if (_entityCache.TryGetValue(id, out var cached))
            {
                if (cached.Length > desiredLength)
                {
                    var temp = new NativeArray<Entity>(desiredLength, Allocator.Persistent,
                        NativeArrayOptions.UninitializedMemory);
                    var excess = new NativeArray<Entity>(cached.Length - desiredLength, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);

                    NativeArray<Entity>.Copy(cached, temp, temp.Length);
                    NativeArray<Entity>.Copy(cached, temp.Length, excess, 0, excess.Length);

                    EntityManager.DestroyEntity(excess);
                    excess.Dispose();

                    _entityCache[id] = temp;
                    cached.Dispose();
                }
                else if (cached.Length < desiredLength)
                {
                    var temp = new NativeArray<Entity>(desiredLength, Allocator.Persistent,
                        NativeArrayOptions.UninitializedMemory);
                    var requested = new NativeArray<Entity>(desiredLength - cached.Length, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);


                    NativeArray<Entity>.Copy(cached, temp, cached.Length);
                    EntityManager.CreateEntity(_archetype, requested);
                    NativeArray<Entity>.Copy(requested, 0, temp, cached.Length, requested.Length);

                    requested.Dispose();

                    _entityCache[id] = temp;
                    cached.Dispose();
                }
            }
            else
            {
                var requested = new NativeArray<Entity>(desiredLength, Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory);
                EntityManager.CreateEntity(_archetype, requested);
                _entityCache[id] = requested;
            }
        }

        void InitializeEntities(NativeArray<Entity> entities, float3 position)
        {
            var rotation = new float3x3(new float3(1, 0, 0), new float3(0, 1, 0), new float3(0, 0, 1));
            foreach (var entity in entities)
            {
                EntityManager.SetComponentData(entity, new Translation() {Value = position});
                EntityManager.SetComponentData(entity, new Rotation() {Value = quaternion.identity});
                //Check if this is still necessary
                EntityManager.SetComponentData(entity, new LocalToWorld() {Value = new float4x4(rotation, position)});
            }
        }

        private struct FrameCache
        {
            public UniversalChunkId Id;
            public UnivoxRenderingJobs.RenderResult[] Results;
        }

        private Queue<FrameCache> FrameCaches;
        private Material _defaultMaterial;

        void RenderPass()
        {
            var chunkArray = _renderQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            var idType = GetArchetypeChunkComponentType<ChunkIdComponent>(true);
            var versionType = GetArchetypeChunkComponentType<ChunkRenderVersion>();


            Profiler.BeginSample("Process ECS Chunk");
            foreach (var ecsChunk in chunkArray)
            {
                var ids = ecsChunk.GetNativeArray(idType);
                var versions = ecsChunk.GetNativeArray(versionType);
                for (var i = 0; i < ecsChunk.Count; i++)
                {
                    var id = ids[i];
                    var version = versions[i];
                    if (!_universe.TryGetValue(id.Value.WorldId, out var world)) continue; //TODO produce an error
                    if (!world.TryGetAccessor(id.Value.ChunkId, out var record)) continue; //TODO produce an error
                    var voxelChunk = record.Chunk;
                    if (!version.DidChange(voxelChunk)) continue; //Skip this chunk

                    //Update version
                    versions[i] = ChunkRenderVersion.Create(voxelChunk);

                    Profiler.BeginSample("Process Render Chunk");
                    var results = UnivoxRenderingJobs.GenerateBoxelMeshes(voxelChunk.Render);
                    Profiler.EndSample();
                    FrameCaches.Enqueue(new FrameCache() {Id = id.Value, Results = results});
                }
            }

            Profiler.EndSample();

            chunkArray.Dispose();

            //We need to process everything we couldn't while chunk array was in use
            ProcessFrameCache();
        }

        void ProcessFrameCache()
        {
            
            Profiler.BeginSample("Create Mesh Entities");
            while (FrameCaches.Count > 0)
            {
                var cached = FrameCaches.Dequeue();
                var id = cached.Id;
                var results = cached.Results;

                var renderEntities = GetResizedCache(id, results.Length);
                InitializeEntities(renderEntities,
                    id.ChunkId * ChunkSize.AxisSize); //ChunkID doubles as unscaled position


                for (var j = 0; j < renderEntities.Length; j++)
                {
                    var renderEntity = renderEntities[j];

                    var meshData = EntityManager.GetSharedComponentData<RenderMesh>(renderEntity);
                    var mesh = results[j].Mesh;
                    var materialId = results[j].Material;

                    var meshVerts = mesh.vertices;
                    var nativeMeshVerts = new NativeArray<float3>(meshVerts.Length, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);
                    for (var i = 0; i < meshVerts.Length; i++)
                        nativeMeshVerts[i] = meshVerts[i];

                    var meshTris = mesh.triangles;
                    var nativeMeshTris = new NativeArray<int>(meshTris.Length, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);
                    for (var i = 0; i < meshTris.Length; i++)
                        nativeMeshTris[i] = meshTris[i];

                    var collider = MeshCollider.Create(nativeMeshVerts, nativeMeshTris, CollisionFilter.Default);
//                    var physSys = EntityManager.World.GetOrCreateSystem<BuildPhysicsWorld>();


                    nativeMeshTris.Dispose();
                    nativeMeshVerts.Dispose();

//                    var physicsData = new PhysicsCollider()
//                    {
//                        Value = new BlobAssetReference<Collider>()
//                    };
//                    var temp = new BlobAssetReference<Collider>()
//                    {
//                        Value = new Collider()
//                        {
//                            Filter = new CollisionFilter()
//                            {
//                                
//                            },
//                        }
//                    };


                    mesh.UploadMeshData(true);
                    meshData.mesh = mesh;

                    GameManager.MasterRegistry.Material.TryGetValue(materialId, out var material);
                    if (material == null)
                        material = _defaultMaterial;
                    meshData.material = material;


                    EntityManager.SetComponentData(renderEntity, new PhysicsCollider() {Value = collider});
                    EntityManager.SetSharedComponentData(renderEntity, meshData);
                }
            }

            Profiler.EndSample();
        }


        void SetupPass()
        {
            EntityManager.AddComponent<ChunkRenderVersion>(_setupQuery);
        }

        void CleanupPass()
        {
            EntityManager.RemoveComponent<ChunkRenderVersion>(_cleanupQuery);
            //TODO, lazy right now, but we need to cleanup the cache
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (_defaultMaterial == null)
                if (!GameManager.MasterRegistry.Material.TryGetValue("Default", out _defaultMaterial))
                    return inputDeps;

            inputDeps.Complete();

            RenderPass();


            CleanupPass();
            SetupPass();


            return new JobHandle();
        }
    }
}