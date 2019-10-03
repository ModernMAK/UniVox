using System.Collections.Generic;
using ECS.UniVox.VoxelChunk.Components;
using ECS.UniVox.VoxelChunk.Systems.ChunkJobs;
using ECS.UniVox.VoxelChunk.Tags;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UniVox;
using UniVox.Types.Identities;
using UniVox.Types.Identities.Voxel;
using UniVox.Utility;
using MeshCollider = Unity.Physics.MeshCollider;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class ChunkMeshGenerationSystem : JobComponentSystem
    {
        private EntityArchetype _archetype;
        private EntityQuery _cleanupQuery;


        private Dictionary<ChunkIdentity, NativeArray<Entity>> _entityCache;
        private Queue<FrameCache> _frameCaches;

        private EntityQuery _renderQuery;


        private ChunkRenderMeshSystem _renderSystem;
        private EntityQuery _setupQuery;


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
            _renderSystem = World.GetOrCreateSystem<ChunkRenderMeshSystem>();
            _renderQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VoxelChunkIdentity>(),
                    ComponentType.ReadWrite<SystemVersion>(),

                    ComponentType.ReadOnly<VoxelBlockMaterialIdentity>(),
                    ComponentType.ReadOnly<VoxelBlockMaterialIdentity.VersionProxyDirty>(),

                    ComponentType.ReadOnly<VoxelBlockSubMaterial>(),
                    ComponentType.ReadOnly<VoxelBlockSubMaterial.VersionProxyDirty>(),


                    ComponentType.ReadOnly<VoxelBlockCullingFlag>(),
                    ComponentType.ReadOnly<VoxelBlockCullingFlag.BlockCullFlagVersion>(),

                    ComponentType.ReadOnly<VoxelActive>(),
                    ComponentType.ReadOnly<BlockActiveVersion>()
                },
                None = new[]
                {
                    ComponentType.ReadOnly<ChunkInvalidTag>()
                }
            });
            _setupQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VoxelChunkIdentity>(),

                    ComponentType.ReadOnly<VoxelBlockMaterialIdentity>(),
                    ComponentType.ReadOnly<VoxelBlockMaterialIdentity.VersionProxyDirty>(),

                    ComponentType.ReadOnly<VoxelBlockSubMaterial>(),
                    ComponentType.ReadOnly<VoxelBlockSubMaterial.VersionProxyDirty>(),

                    ComponentType.ReadOnly<VoxelBlockCullingFlag>(),
                    ComponentType.ReadOnly<VoxelBlockCullingFlag.BlockCullFlagVersion>(),

                    ComponentType.ReadOnly<VoxelActive>(),
                    ComponentType.ReadOnly<BlockActiveVersion>()
                },
                None = new[]
                {
                    ComponentType.ReadWrite<SystemVersion>()
                }
            });
            _cleanupQuery = GetEntityQuery(new EntityQueryDesc
            {
                None = new[]
                {
                    ComponentType.ReadOnly<VoxelChunkIdentity>(),

                    ComponentType.ReadOnly<VoxelBlockMaterialIdentity>(),
                    ComponentType.ReadOnly<VoxelBlockMaterialIdentity.VersionProxyDirty>(),

                    ComponentType.ReadOnly<VoxelBlockSubMaterial>(),
                    ComponentType.ReadOnly<VoxelBlockSubMaterial.VersionProxyDirty>(),

                    ComponentType.ReadOnly<VoxelBlockCullingFlag>(),
                    ComponentType.ReadOnly<VoxelBlockCullingFlag.BlockCullFlagVersion>(),

                    ComponentType.ReadOnly<VoxelActive>(),
                    ComponentType.ReadOnly<BlockActiveVersion>()
                },
                All = new[]
                {
                    ComponentType.ReadWrite<SystemVersion>()
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


        private NativeArray<Entity> GetResizedCache(ChunkIdentity identity, int desiredLength)
        {
            ResizeCache(identity, desiredLength);
            return _entityCache[identity];
        }

        private void ResizeCache(ChunkIdentity identity, int desiredLength)
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

        private void InitializeEntities(NativeArray<Entity> entities, float3 position)
        {
            var rotation = new float3x3(new float3(1, 0, 0), new float3(0, 1, 0), new float3(0, 0, 1));
            foreach (var entity in entities)
            {
                EntityManager.SetComponentData(entity, new Translation {Value = position});
                EntityManager.SetComponentData(entity, new Rotation {Value = quaternion.identity});
                //Check if this is still necessary
                EntityManager.SetComponentData(entity, new LocalToWorld {Value = new float4x4(rotation, position)});
            }
        }


        private void RenderPass()
        {
            var chunkArray = _renderQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            var chunkIdType = GetArchetypeChunkComponentType<VoxelChunkIdentity>(true);
            var systemEntityVersionType = GetArchetypeChunkComponentType<SystemVersion>();

//            var materialType = GetArchetypeChunkBufferType<BlockMaterialIdentityComponent>(true);
//            var subMaterialType = GetArchetypeChunkBufferType<BlockSubMaterialIdentityComponent>(true);
//            var blockShapeType = GetArchetypeChunkBufferType<BlockShapeComponent>(true);
//            var culledFaceType = GetArchetypeChunkBufferType<BlockCulledFacesComponent>(true);

            var materialVersionType =
                GetArchetypeChunkComponentType<VoxelBlockMaterialIdentity.VersionProxyDirty>(true);
            var subMaterialVersionType =
                GetArchetypeChunkComponentType<VoxelBlockSubMaterial.VersionProxyDirty>(true);
            var blockShapeVersionType = GetArchetypeChunkComponentType<VoxelBlockShape.VersionProxyDirty>(true);
            var culledFaceVersionType =
                GetArchetypeChunkComponentType<VoxelBlockCullingFlag.BlockCullFlagVersion>(true);


            var chunkArchetype = GetArchetypeChunkEntityType();
            Profiler.BeginSample("Process ECS Chunk");
            foreach (var ecsChunk in chunkArray)
            {
                var ids = ecsChunk.GetNativeArray(chunkIdType);
                var systemVersions = ecsChunk.GetNativeArray(systemEntityVersionType);
                var materialVersions = ecsChunk.GetNativeArray(materialVersionType);
                var subMaterialVersions = ecsChunk.GetNativeArray(subMaterialVersionType);
                var blockShapeVersions = ecsChunk.GetNativeArray(blockShapeVersionType);
                var culledFaceVersions = ecsChunk.GetNativeArray(culledFaceVersionType);
                var voxelChunkEntityArray = ecsChunk.GetNativeArray(chunkArchetype);


                var i = 0;
                foreach (var voxelChunkEntity in voxelChunkEntityArray)
                {
                    var version = systemVersions[i];
                    var currentVersion = new SystemVersion
                    {
                        Material = materialVersions[i],
                        SubMaterial = subMaterialVersions[i],
                        BlockShape = blockShapeVersions[i],
                        CulledFaces = culledFaceVersions[i]
                    };

                    if (currentVersion.DidChange(version))
                    {
                        var id = ids[i];
                        Profiler.BeginSample("Process Render Chunk");
                        var results = GenerateBoxelMeshes(voxelChunkEntity, new JobHandle());
                        Profiler.EndSample();
                        _frameCaches.Enqueue(new FrameCache {Identity = id, Results = results});

                        systemVersions[i] = currentVersion;
                    }


                    i++;
                }
            }


            Profiler.EndSample();

            chunkArray.Dispose();

            //We need to process everything we couldn't while chunk array was in use
            ProcessFrameCache();
        }


        private RenderResult[] GenerateBoxelMeshes(Entity chunk, JobHandle handle)
        {
            var materialLookup = GetBufferFromEntity<VoxelBlockMaterialIdentity>(true);
            var subMaterialLookup = GetBufferFromEntity<VoxelBlockSubMaterial>(true);
            var blockShapeLookup = GetBufferFromEntity<VoxelBlockShape>(true);
            var culledFaceLookup = GetBufferFromEntity<VoxelBlockCullingFlag>(true);


            var materials = materialLookup[chunk].AsNativeArray();
            var blockShapes = blockShapeLookup[chunk].AsNativeArray();
            var subMaterials = subMaterialLookup[chunk].AsNativeArray();
            var culledFaces = culledFaceLookup[chunk].AsNativeArray();

            const int maxBatchSize = byte.MaxValue;
            handle.Complete();

            Profiler.BeginSample("CreateNative Batches");
            var uniqueBatchData = UnivoxRenderingJobs.GatherUnique(materials);
            Profiler.EndSample();

            var meshes = new RenderResult[uniqueBatchData.Length];
            Profiler.BeginSample("Process Batches");
            for (var i = 0; i < uniqueBatchData.Length; i++)
            {
                var materialId = uniqueBatchData[i];
                Profiler.BeginSample($"Process Batch {i}");
                var gatherPlanerJob = GatherPlanarJob.Create(blockShapes, culledFaces, subMaterials, materials,
                    uniqueBatchData[i], out var queue);
                var gatherPlanerJobHandle = gatherPlanerJob.Schedule(GatherPlanarJob.JobLength, maxBatchSize);

                var writerToReaderJob = new NativeQueueToNativeListJob<PlanarData>
                {
                    OutList = new NativeList<PlanarData>(Allocator.TempJob),
                    Queue = queue
                };
                writerToReaderJob.Schedule(gatherPlanerJobHandle).Complete();
                queue.Dispose();
                var planarBatch = writerToReaderJob.OutList;

                //Calculate the Size Each Voxel Will Use
                var cubeSizeJob = UnivoxRenderingJobs.CreateCalculateCubeSizeJobV2(planarBatch);

                //Calculate the Size of the Mesh and the position to write to per voxel
                var indexAndSizeJob = UnivoxRenderingJobs.CreateCalculateIndexAndTotalSizeJob(cubeSizeJob);
                //Schedule the jobs
                var cubeSizeJobHandle = cubeSizeJob.Schedule(planarBatch.Length, maxBatchSize);
                var indexAndSizeJobHandle = indexAndSizeJob.Schedule(cubeSizeJobHandle);
                //Complete these jobs
                indexAndSizeJobHandle.Complete();

                //GEnerate the mesh
                var genMeshJob = UnivoxRenderingJobs.CreateGenerateCubeBoxelMesh(planarBatch, indexAndSizeJob);
                //Dispose unneccessary native arrays
                indexAndSizeJob.TriangleTotalSize.Dispose();
                indexAndSizeJob.VertexTotalSize.Dispose();
                //Schedule the generation
                var genMeshHandle = genMeshJob.Schedule(planarBatch.Length, maxBatchSize, indexAndSizeJobHandle);

                //Finish and CreateNative the Mesh
                genMeshHandle.Complete();
                planarBatch.Dispose();
                meshes[i] = new RenderResult
                {
                    Mesh = UnivoxRenderingJobs.CreateMesh(genMeshJob),
                    Material = materialId
                };
                Profiler.EndSample();
            }

            Profiler.EndSample();

            uniqueBatchData.Dispose();
            return meshes;
        }

        private void ProcessFrameCache()
        {
            Profiler.BeginSample("CreateNative Mesh Entities");
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
                    var batchId = new BatchGroupIdentity {Chunk = id, MaterialIdentity = materialId};

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


                    nativeMeshTris.Dispose();
                    nativeMeshVerts.Dispose();


                    meshData.CastShadows = ShadowCastingMode.On;
                    meshData.ReceiveShadows = true;
//                    meshData.layer = VoxelLayer //TODO
                    mesh.UploadMeshData(true);
//                    meshData.Mesh = mesh;
                    meshData.Batch = batchId;

                    _renderSystem.UploadMesh(batchId, mesh);


                    EntityManager.SetComponentData(renderEntity, new PhysicsCollider {Value = collider});
                    EntityManager.SetComponentData(renderEntity, meshData);
                }
            }

            Profiler.EndSample();
        }


        private void SetupPass()
        {
            EntityManager.AddComponent<SystemVersion>(_setupQuery);
        }

        private void CleanupPass()
        {
            EntityManager.RemoveComponent<SystemVersion>(_cleanupQuery);
            //TODO, lazy right now, but we need to cleanup the cache
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete();

            RenderPass();


            CleanupPass();
            SetupPass();


            return new JobHandle();
        }

        public struct SystemVersion : ISystemStateComponentData
        {
            public uint CulledFaces;
            public uint BlockShape;
            public uint Material;
            public uint SubMaterial;

            public bool DidChange(SystemVersion version)
            {
                return ChangeVersionUtility.DidChange(CulledFaces, version.CulledFaces) ||
                       ChangeVersionUtility.DidChange(BlockShape, version.BlockShape) ||
                       ChangeVersionUtility.DidChange(Material, version.Material) ||
                       ChangeVersionUtility.DidChange(SubMaterial, version.SubMaterial);
            }

            public override string ToString()
            {
                return $"{CulledFaces}-{BlockShape}-{Material}-{SubMaterial}";
            }
        }

        public struct RenderResult
        {
            public Mesh Mesh;
            public ArrayMaterialIdentity Material;
        }

        private struct FrameCache
        {
            public ChunkIdentity Identity;
            public RenderResult[] Results;
        }
    }
}