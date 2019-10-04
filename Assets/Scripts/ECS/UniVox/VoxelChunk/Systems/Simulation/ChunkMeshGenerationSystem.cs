using System;
using System.Collections.Generic;
using ECS.UniVox.VoxelChunk.Components;
using ECS.UniVox.VoxelChunk.Systems.ChunkJobs;
using ECS.UniVox.VoxelChunk.Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UniVox.Utility;
using Material = UnityEngine.Material;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class ChunkMeshGenerationSystem : JobComponentSystem
    {
        public struct EntityVersion : ISystemStateComponentData, IVersionProxy<EntityVersion>
        {
            public uint CulledFaces;
            public uint BlockShape;
            public uint Material;
            public uint SubMaterial;

            public bool DidChange(EntityVersion version)
            {
                return ChangeVersionUtility.DidChange(CulledFaces, version.CulledFaces) ||
                       ChangeVersionUtility.DidChange(BlockShape, version.BlockShape) ||
                       ChangeVersionUtility.DidChange(Material, version.Material) ||
                       ChangeVersionUtility.DidChange(SubMaterial, version.SubMaterial);
            }

            public EntityVersion GetDirty()
            {
                throw new NotSupportedException();
            }

            public override string ToString()
            {
                return $"{CulledFaces}-{BlockShape}-{Material}-{SubMaterial}";
            }
        }

        private EntityQuery _renderQuery;
        private EntityQuery _setupQuery;
        private EntityQuery _cleanupQuery;


//        private NativeHashMap<BatchGroupIdentity, Entity> _entityCache;
//
        private Queue<MultiJobCache> _jobCache;

//        private Queue<JobWaiter> _jobsWaiting;

        protected override void OnCreate()
        {
            _renderSystem = World.GetOrCreateSystem<ChunkRenderMeshSystem>();
            _renderQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                    ComponentType.ReadWrite<EntityVersion>(),

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
                    ComponentType.ReadWrite<EntityVersion>()
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
                    ComponentType.ReadWrite<EntityVersion>(),
                }
            });

            _jobCache = new Queue<MultiJobCache>(); //Allocator.Persistent);
//            _entityCache = new NativeHashMap<BatchGroupIdentity, Entity>(0,
//                Allocator.Persistent); //<ChunkIdentity, NativeArray<Entity>>();
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
            UnivoxRenderingJobs.NativeMeshContainer[] nmc, JobHandle inputDep)
        {
            var cached = new MultiJobCache()
            {
                Entity = entity,
                NativeMeshes = nmc,
                Identities = identity
            };

            var finalHandle = inputDep;

            cached.WaitHandle = finalHandle;
            _jobCache.Enqueue(cached);
            return finalHandle;
        }


        protected override void OnDestroy()
        {
            _jobCache.DisposeEnumerable();
            //TODO also destroy Entities
//            _entityCache.Dispose();
//            _entityCache.Clear();
        }


        private ChunkRenderMeshSystem _renderSystem;
//        private Material _defaultMaterial;


        JobHandle RenderPass()
        {
            var chunkArray = _renderQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            var chunkIdType = GetArchetypeChunkComponentType<ChunkIdComponent>(true);
            var systemEntityVersionType = GetArchetypeChunkComponentType<EntityVersion>();

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
                    var currentVersion = new EntityVersion()
                    {
                        Material = materialVersions[i],
                        SubMaterial = subMaterialVersions[i],
                        BlockShape = blockShapeVersions[i],
                        CulledFaces = culledFaceVersions[i]
                    };
//                    var matVersion = 
//                    var subMatVersion = 


                    if (currentVersion.DidChange(version))
                    {
                        Profiler.BeginSample("Process Render Chunk");
                        var results = GenerateBoxelMeshes(voxelChunkEntity, ids[i]);
                        outHandles = JobHandle.CombineDependencies(outHandles, results);
                        Profiler.EndSample();
                        systemVersions[i] = currentVersion;
                    }


                    i++;
                }
            }


            Profiler.EndSample();

            chunkArray.Dispose();

            return outHandles;
        }

        /*
         * For a given 
         *
         * 
         */

        [BurstCompile]
        private struct GatherVersionJob : IJob
        {
            [ReadOnly] public ArchetypeChunk Chunk;

            [ReadOnly]
            public ArchetypeChunkComponentType<BlockMaterialIdentityComponent.Version> MaterialIdentityVersionType;

            [ReadOnly] public ArchetypeChunkComponentType<BlockSubMaterialIdentityComponent.Version>
                SubMaterialIdentityVersionType;

            [ReadOnly] public ArchetypeChunkComponentType<BlockShapeComponent.Version> ShapeVersionType;
            [ReadOnly] public ArchetypeChunkComponentType<BlockCulledFacesComponent.Version> CulledVersionType;

            [WriteOnly] public NativeArray<EntityVersion> CurrentVersions;

            public void Execute()
            {
                var materialVersions = Chunk.GetNativeArray(MaterialIdentityVersionType);
                var subMaterialVersions = Chunk.GetNativeArray(SubMaterialIdentityVersionType);
                var blockShapeVersions = Chunk.GetNativeArray(ShapeVersionType);
                var culledFaceVersions = Chunk.GetNativeArray(CulledVersionType);
                for (var i = 0; i < Chunk.Count; i++)
                {
                    CurrentVersions[i] = new EntityVersion()
                    {
                        Material = materialVersions[i],
                        SubMaterial = subMaterialVersions[i],
                        BlockShape = blockShapeVersions[i],
                        CulledFaces = culledFaceVersions[i]
                    };
                }
            }
        }

        JobHandle UpdateVersionAndGatherIgnore(ArchetypeChunk chunk, out NativeArray<bool> ignore,
            JobHandle dependencies)
        {
//            var 
            ignore = new NativeArray<bool>(chunk.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var currentVersions = new NativeArray<EntityVersion>(chunk.Count, Allocator.TempJob);
//            var versionType = 
//            var 


            var gatherVersionJob = new GatherVersionJob()
            {
                Chunk = chunk,
                CulledVersionType = GetArchetypeChunkComponentType<BlockCulledFacesComponent.Version>(true),
                MaterialIdentityVersionType =
                    GetArchetypeChunkComponentType<BlockMaterialIdentityComponent.Version>(true),
                SubMaterialIdentityVersionType =
                    GetArchetypeChunkComponentType<BlockSubMaterialIdentityComponent.Version>(true),
                ShapeVersionType = GetArchetypeChunkComponentType<BlockShapeComponent.Version>(true),
//                CulledVersionType = GetArchetypeChunkComponentType<BlockCulledFacesComponent.Version>(),
//                IdentityVersionType = blockIdentityVersionType,
                CurrentVersions = currentVersions
            }.Schedule(dependencies);

            var gatherIgnoreJob = new GatherDirtyVersionJob<EntityVersion>()
            {
                Chunk = chunk,
                VersionsType = GetArchetypeChunkComponentType<EntityVersion>(),
                CurrentVersions = currentVersions,
                Ignore = ignore,
            }.Schedule(gatherVersionJob);
            var disposeCurrentVersions =
                new DisposeArrayJob<EntityVersion>(currentVersions).Schedule(gatherIgnoreJob);

            return disposeCurrentVersions;
        }


        JobHandle CreateBatches<T>(ArchetypeChunk chunk, NativeArray<bool> ignore,
            out FindAndCountUniquePerChunk<T> job, JobHandle dependencies)
            where T : struct, IBufferElementData, IComparable<T>
        {
//            var batchBuffer = GetBufferFromEntity<T>(true);

//            results = new NativeList<T>(Allocator.TempJob);
            job = new FindAndCountUniquePerChunk<T>()
            {
                BufferFromEntity = GetBufferFromEntity<T>(true),
                Chunk = chunk,
                Counts = new NativeList<int>(Allocator.TempJob),
                Offsets = new NativeList<int>(Allocator.TempJob),
                EntityType = GetArchetypeChunkEntityType(),
                Ignore = ignore,

//                Source = batchBuffer[entity].AsNativeArray(),
                Unique = new NativeList<T>(Allocator.TempJob)
            };
            return job.Schedule(dependencies);
        }

        [BurstCompile]
        public struct FindAndCountUniquePerChunk<T> : IJob where T : struct, IComparable<T>, IBufferElementData
//, IEquatable<T>
        {
            [ReadOnly] public ArchetypeChunk Chunk;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            public BufferFromEntity<T> BufferFromEntity;

            [ReadOnly] public NativeArray<bool> Ignore;

//        public NativeArray<T> Source;
            public NativeList<T> Unique;
            public NativeList<int> Offsets;
            public NativeList<int> Counts;


            private int Execute(Entity entity, int previousOFfset)
            {
                var buffer = BufferFromEntity[entity];
                var count = 0;
                for (var bufferIndex = 0; bufferIndex < buffer.Length; bufferIndex++)
                {
                    if (Insert(buffer[bufferIndex], previousOFfset, count))
                        count++;
                }

                return count;
            }

            public void Execute()
            {
                var entities = Chunk.GetNativeArray(EntityType);
                var prevOffset = 0;
                for (var entityIndex = 0; entityIndex < entities.Length; entityIndex++)
                {
                    if (Ignore[entityIndex])
                    {
//                        Offsets[entityIndex] = 0;
//                        Counts[entityIndex] = 0;
                        continue;
                    }

                    var count = Execute(entities[entityIndex], prevOffset);

                    Offsets.Add(prevOffset + count);
                    Counts.Add(count);
                    prevOffset += count;
                }

//            temp
//            for (var i = 0; i < Source.Length; i++)
//            {
//                Insert(Source[i]);
//            }
            }


            #region UniqueList Helpers

            private bool RawFind(T value, int min, int max)
            {
                while (true)
                {
                    if (min > max) return false;

                    var mid = (min + max) / 2;

                    var delta = value.CompareTo(Unique[mid]);

                    if (delta == 0)
                        return true;
                    if (delta < 0) //-
                    {
                        max = mid - 1;
                    }
                    else //+
                    {
                        min = mid + 1;
                    }
                }
            }


            private bool Insert(T value, int start, int len)
            {
                return RawInsert(value, start, start + len - 1);
            }

            private bool RawInsert(T value, int min, int max)
            {
                while (true)
                {
                    if (min > max)
                    {
                        //Max is our min, and min is our max, so lets fix that real quick
                        var temp = max;
                        max = min;
                        min = temp;
//                    if (min < -1) //Allow -1, to insert at front
//                        min = -1;
//                    else if (min > Unique.Length - 1)
//                        min = Unique.Length - 1;


                        var insertAt = min + 1;


                        //Add a placeholder
                        Unique.Add(default);
                        //Shift all elements down one
                        for (var i = insertAt + 1; i < Unique.Length; i++)
                            Unique[i] = Unique[i - 1];
                        //Insert the correct element
                        Unique[insertAt] = value;
                        return true;
                    }

                    var mid = (min + max) / 2;

                    var delta = value.CompareTo(Unique[mid]);

                    if (delta == 0)
                        return false; //Material is Unique and present
                    if (delta < 0) //-
                    {
                        max = mid - 1;
                    }
                    else //+
                    {
                        min = mid + 1;
                    }
                }
            }

            #endregion
        }



        JobHandle GenerateBoxelMeshes(ArchetypeChunk chunk, JobHandle dependencies)
        {
            var updateAndIgnore = UpdateVersionAndGatherIgnore(chunk, out var ignore, dependencies);
            var findUnique =
                CreateBatches<BlockMaterialIdentityComponent>(chunk, ignore, out var uniqueJob, updateAndIgnore);

            return findUnique;
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
            //Gather unique materials, these become our batch groups
            var uniqueBatchData = new NativeList<BlockMaterialIdentityComponent>(Allocator.TempJob);
            var job = new FindUniquesJob<BlockMaterialIdentityComponent>()
            {
                Source = materials,
                Unique = uniqueBatchData
            };


//            var uniqueJob = CreateBatches<BlockMaterialIdentityComponent>(chunk, out var batches, handle);

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
                var disposePlanar = new DisposeArrayJob<PlanarData>(planarBatch).Schedule(genMeshHandle);
//                var disposePlanar = new DisposeListJob<PlanarData>(planarBatch).Sc
//
// hedule(genMeshHandle);
                outHandle = JobHandle.CombineDependencies(outHandle, disposePlanar);
                //Finish and CreateNative the Mesh
//                genMeshHandle.Complete();
                planarBatch.Dispose();

                Profiler.EndSample();
            }

            var cacheHandle = AddToCache(chunk, batches, nativeMeshes, outHandle);
//            outHandle = JobHandle.CombineDependencies(outHandle, cacheHandle);
            Profiler.EndSample();

            uniqueBatchData.Dispose();
            return outHandle;
        }


        void SetupPass()
        {
            EntityManager.AddComponent<EntityVersion>(_setupQuery);
        }

        void CleanupPass()
        {
            EntityManager.RemoveComponent<EntityVersion>(_cleanupQuery);
            //TODO, lazy right now, but we need to cleanup the cache
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete();


            CleanupPass();
            SetupPass();

            ProcessJobCache();

            return RenderPass();

//            return new JobHandle();
        }
    }
}