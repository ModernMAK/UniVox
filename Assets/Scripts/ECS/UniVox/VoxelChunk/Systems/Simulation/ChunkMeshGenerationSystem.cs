using System;
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
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UniVox.Types.Native;
using UniVox.Utility;
using Material = UnityEngine.Material;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class ChunkMeshGenerationSystem : JobComponentSystem
    {
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

        private EntityQuery _renderQuery;
        private EntityQuery _setupQuery;
        private EntityQuery _cleanupQuery;

//        private Universe _universe;
//        private EntityCommandBufferSystem _updateEnd;

//        private Dictionary<ChunkIdentity, NativeArray<Entity>> _entityCache;
//        private EntityArchetype _chunkRenderArchetype;


        private NativeHashMap<BatchGroupIdentity, Entity> _entityCache;

        //TODO remove the need for this
        private Queue<MultiJobCache> _jobCache;

//
//        private void SetupArchetype()
//        {
////            _chunkRenderArchetype = EntityManager.CreateArchetype(
////                //Rendering
////                typeof(ChunkRenderMesh),
////                typeof(LocalToWorld),
//////Physics
////                typeof(Translation),
////                typeof(Rotation),
////                typeof(PhysicsCollider)
////            );
////            _chunkRenderEventityArchetype = EntityManager.CreateArchetype(
////                typeof(VertexBufferComponent),
////                typeof(NormalBufferComponent),
////                typeof(TextureMap0BufferComponent),
////                typeof(TangentBufferComponent),
////                typeof(CreateChunkMeshEventity)
////            );
//        }

        protected override void OnCreate()
        {
//            _updateEnd = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
////            _frameCaches = new Queue<FrameCache>();
////            _universe = GameManager.Universe;
//            SetupArchetype();
            _renderSystem = World.GetOrCreateSystem<ChunkRenderMeshSystem>();
            _renderQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                    ComponentType.ReadWrite<SystemVersion>(),

                    ComponentType.ReadOnly<BlockMaterialIdentityComponent>(),
                    ComponentType.ReadOnly<BlockMaterialIdentityComponent.Version>(),

                    ComponentType.ReadOnly<BlockSubMaterialIdentityComponent>(),
                    ComponentType.ReadOnly<BlockSubMaterialIdentityComponent.Version>(),


                    ComponentType.ReadOnly<BlockCulledFacesComponent>(),
                    ComponentType.ReadOnly<BlockCulledFacesComponent.Version>(),

                    ComponentType.ReadOnly<BlockActiveComponent>(),
                    ComponentType.ReadOnly<BlockActiveComponent.Version>(),

                    ComponentType.ReadWrite<PhysicsCollider>(),
                    ComponentType.ReadWrite<ChunkMeshBuffer>(),
                },
                None = new[]
                {
                    ComponentType.ReadOnly<ChunkInvalidTag>(),
                }
            });
            _setupQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),

                    ComponentType.ReadOnly<BlockMaterialIdentityComponent>(),
                    ComponentType.ReadOnly<BlockMaterialIdentityComponent.Version>(),

                    ComponentType.ReadOnly<BlockSubMaterialIdentityComponent>(),
                    ComponentType.ReadOnly<BlockSubMaterialIdentityComponent.Version>(),

                    ComponentType.ReadOnly<BlockCulledFacesComponent>(),
                    ComponentType.ReadOnly<BlockCulledFacesComponent.Version>(),

                    ComponentType.ReadOnly<BlockActiveComponent>(),
                    ComponentType.ReadOnly<BlockActiveComponent.Version>(),

                    ComponentType.ReadWrite<PhysicsCollider>(),
                    ComponentType.ReadWrite<ChunkMeshBuffer>(),
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
                    ComponentType.ReadOnly<BlockMaterialIdentityComponent.Version>(),

                    ComponentType.ReadOnly<BlockSubMaterialIdentityComponent>(),
                    ComponentType.ReadOnly<BlockSubMaterialIdentityComponent.Version>(),

                    ComponentType.ReadOnly<BlockCulledFacesComponent>(),
                    ComponentType.ReadOnly<BlockCulledFacesComponent.Version>(),

                    ComponentType.ReadOnly<BlockActiveComponent>(),
                    ComponentType.ReadOnly<BlockActiveComponent.Version>(),

                    ComponentType.ReadWrite<PhysicsCollider>(),
                    ComponentType.ReadWrite<ChunkMeshBuffer>(),
                },
                All = new[]
                {
                    ComponentType.ReadWrite<SystemVersion>(),
                }
            });

            _jobCache = new Queue<MultiJobCache>(); //Allocator.Persistent);
            _entityCache = new NativeHashMap<BatchGroupIdentity, Entity>(0,
                Allocator.Persistent); //<ChunkIdentity, NativeArray<Entity>>();
        }

        private struct MultiJobCache : IDisposable
        {
            public JobHandle WaitHandle;

            public BatchGroupIdentity[] Identities;


            public Entity Entity;

//            public NativeValue<Entity> Entity;
//            public Entity GetEntity(NativeHashMap<BatchGroupIdentity, Entity> lookup) => lookup[Identity];
            public UnivoxRenderingJobs.NativeMeshContainer[] NativeMeshes;

            public bool IsHandleCompleted()
            {
                return WaitHandle.IsCompleted; // && Entity.Value != Unity.Entities.Entity.Null;
            }

            public void Dispose()
            {
//                Entity.Dispose();
                foreach (var nativeMesh in NativeMeshes)
                {
                    nativeMesh.Dispose();
                }
            }
        }

        private struct JobCache : IDisposable
        {
            public JobHandle WaitHandle;

            public BatchGroupIdentity Identity;


            public Entity Entity;

//            public NativeValue<Entity> Entity;
//            public Entity GetEntity(NativeHashMap<BatchGroupIdentity, Entity> lookup) => lookup[Identity];
            public UnivoxRenderingJobs.NativeMeshContainer NativeMesh;

//            public bool IsValid(NativeHashMap<BatchGroupIdentity, Entity> lookup)
//            {
//                if (!lookup.TryGetValue(Identity, out var entity)) return false;
//
//
//                var handleCompleted = IsHandleCompleted();
//                var entityNotNull = !IsEntityNull(entity);
//                var entityNotDeferred = !IsEntityDeferred(entity);
//
//                return handleCompleted && entityNotDeferred && entityNotNull;
//            }

            public bool IsHandleCompleted()
            {
                return WaitHandle.IsCompleted; // && Entity.Value != Unity.Entities.Entity.Null;
            }

            public void Dispose()
            {
//                Entity.Dispose();
                NativeMesh.Dispose();
            }
        }

        void ProcessJobCache()
        {
            var getBuffer = GetBufferFromEntity<ChunkMeshBuffer>();
            var len = _jobCache.Count;
            for (var i = 0; i < len; i++)
            {
                var cached = _jobCache.Dequeue();

                if (!cached.IsHandleCompleted())
                {
                    _jobCache.Enqueue(cached);
                    continue;
                }

                var entity = cached.Entity; //.GetEntity(_entityCache);

                var buffer = getBuffer[entity];

                buffer.ResizeUninitialized(cached.NativeMeshes.Length);
                for (var j = 0; j < cached.NativeMeshes.Length; j++)
                {
                    var id = cached.Identities[j];
                    var nativeMesh = cached.NativeMeshes[j];
                    var mesh = UnivoxRenderingJobs.CreateMesh(nativeMesh);
                    _renderSystem.UploadMesh(id, mesh);
                    mesh.UploadMeshData(true);


                    var meshData = buffer[j];
//                    var meshData = EntityManager.GetComponentData<ChunkRenderMesh>(entity);


                    meshData.CastShadows = ShadowCastingMode.On;
                    meshData.ReceiveShadows = true;
                    meshData.Layer = 0; // = VoxelLayer //TODO

                    meshData.SubMesh = 0;
                    meshData.Batch = id;
                    buffer[j] = meshData;
//                    EntityManager.SetComponentData(entity, meshData);
                }

                var collider = UnivoxRenderingJobs.CreateMeshCollider(cached.NativeMeshes);
                EntityManager.SetComponentData(entity, new PhysicsCollider() {Value = collider});

                cached.Dispose();
            }
        }

        JobHandle AddToCache(Entity entity, BatchGroupIdentity[] identity,
            UnivoxRenderingJobs.NativeMeshContainer[] nmc,
            JobHandle inputDep)
        {
            var cached = new MultiJobCache()
            {
                Entity = entity,
//                Entity = new NativeValue<Entity>(Allocator.Persistent),
                NativeMeshes = nmc,
                Identities = identity
            };

//            var mergedHandle = JobHandle.CombineDependencies(inputDep, _entityCacheHandle);
//            var commandBuffer = _updateEnd.CreateCommandBuffer();

            var finalHandle = new JobHandle(); //initEntity;

            cached.WaitHandle = finalHandle;
            _jobCache.Enqueue(cached);
            return finalHandle;
        }


        protected override void OnDestroy()
        {
            _jobCache.DisposeEnumerable();
            //TODO also destroy Entities
            _entityCache.Dispose();
//            _entityCache.Clear();
        }


        private ChunkRenderMeshSystem _renderSystem;
        private Material _defaultMaterial;


        JobHandle RenderPass()
        {
            var chunkArray = _renderQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            var chunkIdType = GetArchetypeChunkComponentType<ChunkIdComponent>(true);
            var systemEntityVersionType = GetArchetypeChunkComponentType<SystemVersion>();

//            var materialType = GetArchetypeChunkBufferType<BlockMaterialIdentityComponent>(true);
//            var subMaterialType = GetArchetypeChunkBufferType<BlockSubMaterialIdentityComponent>(true);
//            var blockShapeType = GetArchetypeChunkBufferType<BlockShapeComponent>(true);
//            var culledFaceType = GetArchetypeChunkBufferType<BlockCulledFacesComponent>(true);

            var materialVersionType = GetArchetypeChunkComponentType<BlockMaterialIdentityComponent.Version>(true);
            var subMaterialVersionType =
                GetArchetypeChunkComponentType<BlockSubMaterialIdentityComponent.Version>(true);
            var blockShapeVersionType = GetArchetypeChunkComponentType<BlockShapeComponent.Version>(true);
            var culledFaceVersionType = GetArchetypeChunkComponentType<BlockCulledFacesComponent.Version>(true);

            JobHandle outHandles = new JobHandle();

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
                    var currentVersion = new SystemVersion()
                    {
                        Material = materialVersions[i],
                        SubMaterial = subMaterialVersions[i],
                        BlockShape = blockShapeVersions[i],
                        CulledFaces = culledFaceVersions[i]
                    };
//                    var matVersion = 
//                    var subMatVersion = 


                    if (currentVersion.DidChange(version))
//                    .DidChange(materialType, version.Material) ||
//                        ecsChunk.DidChange(subMaterialType, version.SubMaterial) ||
//                        ecsChunk.DidChange(culledFaceType, version.CulledFaces) ||
//                        ecsChunk.DidChange(blockShapeType, version.BlockShape))
                    {
//                        var id = ids[i];
                        Profiler.BeginSample("Process Render Chunk");
                        var results = GenerateBoxelMeshes(voxelChunkEntity, ids[i]);
                        outHandles = JobHandle.CombineDependencies(outHandles, results);
                        Profiler.EndSample();
//                        _frameCaches.Enqueue(new FrameCache() {Key = id, Results = results});

                        systemVersions[i] = currentVersion;
                    }


                    i++;
                }
            }


            Profiler.EndSample();

            chunkArray.Dispose();

            return outHandles;
        }


        JobHandle GenerateBoxelMeshes(Entity chunk, ChunkIdComponent chunkPos, JobHandle handle = default)
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

            Profiler.BeginSample("CreateNative Batches");
            var uniqueBatchData = UnivoxRenderingJobs.GatherUnique(materials);
            Profiler.EndSample();

            var outHandle = new JobHandle();

            var nativeMeshes = new UnivoxRenderingJobs.NativeMeshContainer[uniqueBatchData.Length];
            var batches = new BatchGroupIdentity[uniqueBatchData.Length];
//            var meshes = new UnivoxRenderingJobs.RenderResult[uniqueBatchData.Length];
            Profiler.BeginSample("Process Batches");
            for (var i = 0; i < uniqueBatchData.Length; i++)
            {
                Profiler.BeginSample($"Process Batch {i}");
                var gatherPlanerJob = GatherPlanarJob.Create(blockShapes, culledFaces, subMaterials, materials,
                    uniqueBatchData[i], out var queue);
                var gatherPlanerJobHandle = gatherPlanerJob.Schedule(GatherPlanarJob.JobLength, maxBatchSize);

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

                //Calculate the Size of the Mesh and the Position to write to per voxel
                var indexAndSizeJob = UnivoxRenderingJobs.CreateCalculateIndexAndTotalSizeJob(cubeSizeJob);
                //Schedule the jobs
                var cubeSizeJobHandle = cubeSizeJob.Schedule(planarBatch.Length, maxBatchSize);
                var indexAndSizeJobHandle = indexAndSizeJob.Schedule(cubeSizeJobHandle);
                //Complete these jobs
                indexAndSizeJobHandle.Complete();

                //GEnerate the mesh
//                var genMeshJob = CreateGenerateCubeBoxelMesh(planarBatch, offsets, indexAndSizeJob);
                var genMeshJob =
                    UnivoxRenderingJobs.CreateGenerateCubeBoxelMesh(planarBatch, indexAndSizeJob,
                        out var nativeMeshContainer, Allocator.Persistent);
                nativeMeshes[i] = nativeMeshContainer;
                //DisposeEnumerable unneccessary native arrays
                indexAndSizeJob.TriangleTotalSize.Dispose();
                indexAndSizeJob.VertexTotalSize.Dispose();
                //Schedule the generation
                var genMeshHandle = genMeshJob.Schedule(planarBatch.Length, maxBatchSize, indexAndSizeJobHandle);
                batches[i] = new BatchGroupIdentity()
                {
                    Chunk = chunkPos,
                    MaterialIdentity = uniqueBatchData[i]
                };
                //Finish and CreateNative the Mesh
                genMeshHandle.Complete();
                planarBatch.Dispose();


//                var eventityCreate = CreateMeshEventity(genMeshJob, genMeshHandle);
//                outHandle = JobHandle.CombineDependencies(outHandle, eventityCreate);
//                meshes[i] = new UnivoxRenderingJobs.RenderResult()
//                {
//                    Mesh = UnivoxRenderingJobs.CreateMesh(genMeshJob),
//                    Material = materialId
//                };
                Profiler.EndSample();
            }

//
//            var batchId = new BatchGroupIdentity()
//            {
//            };
            var cacheHandle = AddToCache(chunk, batches, nativeMeshes, new JobHandle());
            outHandle = JobHandle.CombineDependencies(outHandle, cacheHandle);
            Profiler.EndSample();

//            offsets.DisposeEnumerable();
            uniqueBatchData.Dispose();
            return outHandle;
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


            CleanupPass();
            SetupPass();

            ProcessJobCache();

            return RenderPass();

//            return new JobHandle();
        }
    }
}