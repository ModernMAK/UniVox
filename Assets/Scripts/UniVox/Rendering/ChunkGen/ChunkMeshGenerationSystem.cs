using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UniVox.Launcher;
using UniVox.Managers.Game;
using UniVox.Rendering.ChunkGen.Jobs;
using UniVox.Rendering.Render;
using UniVox.Types;
using UniVox.Utility;
using UniVox.VoxelData;
using UniVox.VoxelData.Chunk_Components;
using Material = UnityEngine.Material;
using MeshCollider = Unity.Physics.MeshCollider;

namespace UniVox.Rendering.ChunkGen
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class ChunkMeshGenerationSystem : JobComponentSystem
    {
        public struct SystemVersion : ISystemStateComponentData
        {
            public uint CulledFaces;
            public uint BlockShape;
            public uint Material;
            public uint SubMaterial;
        }

        private EntityQuery _renderQuery;
        private EntityQuery _setupQuery;
        private EntityQuery _cleanupQuery;

//        private Universe _universe;

        private Dictionary<ChunkIdentity, NativeArray<Entity>> _entityCache;
        private EntityArchetype _archetype;

        private void SetupArchetype()
        {
            _archetype = EntityManager.CreateArchetype(
                //Rendering
                typeof(ChunkRenderMesh),
                typeof(LocalToWorld),
//Physics
                typeof(Translation),
                typeof(Rotation),
                typeof(PhysicsCollider));
        }

        protected override void OnCreate()
        {
            _frameCaches = new Queue<FrameCache>();
//            _universe = GameManager.Universe;
            SetupArchetype();
            _renderSystem = World.GetOrCreateSystem<ChunkRenderMeshSystemV3>();
            _renderQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                    ComponentType.ReadWrite<SystemVersion>(),
                    ComponentType.ReadOnly<BlockMaterialIdentityComponent>(),
                    ComponentType.ReadOnly<BlockSubMaterialIdentityComponent>(),
                }
            });
            _setupQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                    ComponentType.ReadOnly<BlockMaterialIdentityComponent>(),
                    ComponentType.ReadOnly<BlockSubMaterialIdentityComponent>(),
                },
                None = new[]
                {
                    ComponentType.ReadWrite<SystemVersion>()
                }
            });
            _cleanupQuery = GetEntityQuery(new EntityQueryDesc()
            {
                None = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                    ComponentType.ReadOnly<BlockMaterialIdentityComponent>(),
                    ComponentType.ReadOnly<BlockSubMaterialIdentityComponent>(),
                },
                All = new[]
                {
                    ComponentType.ReadWrite<SystemVersion>(),
                }
            });

            _entityCache = new Dictionary<ChunkIdentity, NativeArray<Entity>>();
        }

        protected override void OnDestroy()
        {
            //TODO also destroy Entities
            _entityCache.Dispose();
            _entityCache.Clear();
        }


        NativeArray<Entity> GetResizedCache(ChunkIdentity identity, int desiredLength)
        {
            ResizeCache(identity, desiredLength);
            return _entityCache[identity];
        }

        void ResizeCache(ChunkIdentity identity, int desiredLength)
        {
            if (_entityCache.TryGetValue(identity, out var cached))
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

                    _entityCache[identity] = temp;
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

                    _entityCache[identity] = temp;
                    cached.Dispose();
                }
            }
            else
            {
                var requested = new NativeArray<Entity>(desiredLength, Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory);
                EntityManager.CreateEntity(_archetype, requested);
                _entityCache[identity] = requested;
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
            public ChunkIdentity Identity;
            public UnivoxRenderingJobs.RenderResult[] Results;
        }


        private ChunkRenderMeshSystemV3 _renderSystem;
        private Queue<FrameCache> _frameCaches;
        private Material _defaultMaterial;

        void RenderPass()
        {
            var chunkArray = _renderQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            var idType = GetArchetypeChunkComponentType<ChunkIdComponent>(true);
            var versionType = GetArchetypeChunkComponentType<SystemVersion>();

            var materialType = GetArchetypeChunkBufferType<BlockMaterialIdentityComponent>(true);
            var subMaterialType = GetArchetypeChunkBufferType<BlockSubMaterialIdentityComponent>(true);
            var blockShapeType = GetArchetypeChunkBufferType<BlockShapeComponent>(true);
            var culledFaceType = GetArchetypeChunkBufferType<BlockCulledFacesComponent>(true);


            var chunkArchetype = GetArchetypeChunkEntityType();
            Profiler.BeginSample("Process ECS Chunk");
            foreach (var ecsChunk in chunkArray)
            {
                var ids = ecsChunk.GetNativeArray(idType);
                var versions = ecsChunk.GetNativeArray(versionType);
                var voxelChunkEntityArray = ecsChunk.GetNativeArray(chunkArchetype);
                var i = 0;
                foreach (var voxelChunkEntity in voxelChunkEntityArray)
                {
                    var version = versions[i];
//                    var matVersion = 
//                    var subMatVersion = 

                    if (ecsChunk.DidChange(materialType, version.Material) ||
                        ecsChunk.DidChange(subMaterialType, version.SubMaterial) ||
                        ecsChunk.DidChange(culledFaceType, version.CulledFaces) ||
                        ecsChunk.DidChange(blockShapeType, version.BlockShape))
                    {
                        var id = ids[i];
                        Profiler.BeginSample("Process Render Chunk");
                        var results = GenerateBoxelMeshes(voxelChunkEntity);
                        Profiler.EndSample();
                        _frameCaches.Enqueue(new FrameCache() {Identity = id, Results = results});

                        versions[i] = new SystemVersion()
                        {
                            Material = ecsChunk.GetComponentVersion(materialType),
                            SubMaterial = ecsChunk.GetComponentVersion(subMaterialType),
                            BlockShape = ecsChunk.GetComponentVersion(blockShapeType),
                            CulledFaces = ecsChunk.GetComponentVersion(culledFaceType),
                        };
                    }


                    i++;
                }
            }


            Profiler.EndSample();

            chunkArray.Dispose();

            //We need to process everything we couldn't while chunk array was in use
            ProcessFrameCache();
        }

        UnivoxRenderingJobs.RenderResult[] GenerateBoxelMeshes(Entity chunk, JobHandle handle = default)
        {
            var materialLookup = GetBufferFromEntity<BlockMaterialIdentityComponent>(true);
            var subMaterialLookup = GetBufferFromEntity<BlockSubMaterialIdentityComponent>(true);
            var blockShapeLookup = GetBufferFromEntity<BlockShapeComponent>(true);
            var culledFaceLookup = GetBufferFromEntity<BlockCulledFacesComponent>(true);


            var materials = materialLookup[chunk].AsNativeArray();
            var blockShapes = blockShapeLookup[chunk].AsNativeArray();
            var subMaterials = subMaterialLookup[chunk].AsNativeArray();
            var culledFaces = culledFaceLookup[chunk].AsNativeArray();

            const int maxBatchSize = Byte.MaxValue;
            handle.Complete();

            Profiler.BeginSample("Create Batches");
            var uniqueBatchData = UnivoxRenderingJobs.GatherUnique(materials);
            Profiler.EndSample();

            var meshes = new UnivoxRenderingJobs.RenderResult[uniqueBatchData.Length];
            Profiler.BeginSample("Process Batches");
            for (var i = 0; i < uniqueBatchData.Length; i++)
            {
                var materialId = uniqueBatchData[i];
                Profiler.BeginSample($"Process Batch {i}");
                var gatherPlanerJob = GatherPlanarJobV3.Create(blockShapes, culledFaces, subMaterials, materials,
                    uniqueBatchData[i], out var queue);
                var gatherPlanerJobHandle = gatherPlanerJob.Schedule(GatherPlanarJobV3.JobLength, maxBatchSize);

                var writerToReaderJob = new NativeQueueToNativeListJob<PlanarData>()
                {
                    OutList = new NativeList<PlanarData>(Allocator.TempJob),
                    Queue = queue
                };
                writerToReaderJob.Schedule(gatherPlanerJobHandle).Complete();
                queue.Dispose();
                var planarBatch = writerToReaderJob.OutList;

                //Calculate the Size Each Voxel Will Use
//                var cubeSizeJob = CreateCalculateCubeSizeJob(batch, chunk);
                var cubeSizeJob = UnivoxRenderingJobs.CreateCalculateCubeSizeJobV2(planarBatch);

                //Calculate the Size of the Mesh and the position to write to per voxel
                var indexAndSizeJob = UnivoxRenderingJobs.CreateCalculateIndexAndTotalSizeJob(cubeSizeJob);
                //Schedule the jobs
                var cubeSizeJobHandle = cubeSizeJob.Schedule(planarBatch.Length, maxBatchSize);
                var indexAndSizeJobHandle = indexAndSizeJob.Schedule(cubeSizeJobHandle);
                //Complete these jobs
                indexAndSizeJobHandle.Complete();

                //GEnerate the mesh
//                var genMeshJob = CreateGenerateCubeBoxelMeshV2(planarBatch, offsets, indexAndSizeJob);
                var genMeshJob = UnivoxRenderingJobs.CreateGenerateCubeBoxelMeshV2(planarBatch, indexAndSizeJob);
                //Dispose unneccessary native arrays
                indexAndSizeJob.TriangleTotalSize.Dispose();
                indexAndSizeJob.VertexTotalSize.Dispose();
                //Schedule the generation
                var genMeshHandle =
                    genMeshJob.Schedule(planarBatch.Length, maxBatchSize, indexAndSizeJobHandle);

                //Finish and Create the Mesh
                genMeshHandle.Complete();
                planarBatch.Dispose();
                meshes[i] = new UnivoxRenderingJobs.RenderResult()
                {
                    Mesh = UnivoxRenderingJobs.CreateMesh(genMeshJob),
                    Material = materialId
                };
                Profiler.EndSample();
            }

            Profiler.EndSample();

//            offsets.Dispose();
            uniqueBatchData.Dispose();
            return meshes;
        }

        void ProcessFrameCache()
        {
            Profiler.BeginSample("Create Mesh Entities");
            while (_frameCaches.Count > 0)
            {
                var cached = _frameCaches.Dequeue();
                var id = cached.Identity;
                var results = cached.Results;

                var renderEntities = GetResizedCache(id, results.Length);
                InitializeEntities(renderEntities,
                    id.ChunkId * UnivoxDefine.AxisSize); //ChunkID doubles as unscaled position


                for (var j = 0; j < renderEntities.Length; j++)
                {
                    var renderEntity = renderEntities[j];


                    var meshData = EntityManager.GetComponentData<ChunkRenderMesh>(renderEntity);
                    var mesh = results[j].Mesh;
                    var materialId = results[j].Material;
                    var batchId = new BatchGroupIdentity() {Chunk = id, MaterialId = materialId};

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
//                        Material = new BlobAssetReference<Collider>()
//                    };
//                    var temp = new BlobAssetReference<Collider>()
//                    {
//                        Material = new Collider()
//                        {
//                            Filter = new CollisionFilter()
//                            {
//                                
//                            },
//                        }
//                    };


                    meshData.CastShadows = ShadowCastingMode.On;
                    meshData.ReceiveShadows = true;
//                    meshData.layer = VoxelLayer //TODO
                    mesh.UploadMeshData(true);
//                    meshData.Mesh = mesh;
                    meshData.Batch = batchId;

                    _renderSystem.UploadMesh(batchId, mesh);
//                    if (GameManager.Registry.ArrayMaterials.TryGetValue(materialId, out var arrayMaterial))
//                    {
//                        meshData.material = arrayMaterial.Material;
//                    }
//                    else
//                    {
//                        meshData.material = _defaultMaterial;
//                    }


                    EntityManager.SetComponentData(renderEntity, new PhysicsCollider() {Value = collider});
                    EntityManager.SetComponentData(renderEntity, meshData);
                }
            }

            Profiler.EndSample();
        }


        void SetupPass()
        {
            EntityManager.AddComponent<SystemVersion>(_setupQuery);
        }

        void CleanupPass()
        {
            EntityManager.RemoveComponent<SystemVersion>(_cleanupQuery);
            //TODO, lazy right now, but we need to cleanup the cache
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
//            if (_defaultMaterial == null)
//            {
//                if (!GameManager.Registry.ArrayMaterials.TryGetValue(
//                    new ArrayMaterialKey(BaseGameMod.ModPath, "Default"), out var defaultMaterial))
//                    return inputDeps;
//                else
//                    _defaultMaterial = defaultMaterial.Material;
//            }

            inputDeps.Complete();

            RenderPass();


            CleanupPass();
            SetupPass();


            return new JobHandle();
        }
    }
}