using System;
using System.Collections.Generic;
using ECS.UniVox.VoxelChunk.Components;
using ECS.UniVox.VoxelChunk.Systems.ChunkJobs;
using ECS.UniVox.VoxelChunk.Tags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UniVox.Rendering.MeshPrefabGen;
using UniVox.Types.Identities;
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

            public UnivoxRenderingJobs.DynamicNativeMeshContainer DNMC;
            public NativeList<int> VertexCounts;
            public NativeList<int> VertexOffsets;
            public NativeList<int> TriangleOffsets;
            public NativeList<int> TriangleCounts;
            public NativeList<BatchGroupIdentity> Identities;
            public NativeArray<bool> Ignores;
            public NativeArray<Entity> Entities;
            public NativeArray<int> BatchCounts;


//            public NativeValue<Entity> Entity;
//            public Entity GetEntity(NativeHashMap<BatchGroupIdentity, Entity> lookup) => lookup[Identity];
//            public UnivoxRenderingJobs.NativeMeshContainer[] NativeMeshes;

            public bool IsHandleCompleted()
            {
                return WaitHandle.IsCompleted; // && Entity.Value != Unity.Entities.Entity.Null;
            }

            public void Dispose()
            {
                DNMC.Dispose();
                VertexCounts.Dispose();
                VertexOffsets.Dispose();
                TriangleOffsets.Dispose();
                TriangleCounts.Dispose();
                Identities.Dispose();
                Ignores.Dispose();
                Entities.Dispose();
                BatchCounts.Dispose();


//              
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

                //Iterate over Chunk (entities)
                var entities = cached.Entities;
                var runningTotal = 0;
                for (var j = 0; j < entities.Length; j++)
                {
                    if (cached.Ignores[j])
                        continue;

                    var entity = entities[j]; //.GetEntity(_entityCache);

                    var buffer = getBuffer[entity];

                    var batches = cached.BatchCounts[j];
                    buffer.ResizeUninitialized(batches);

                    //Iterate over batches

                    using (var verts = new NativeList<float3>(Allocator.Temp))

                    using (var indexes = new NativeList<int>(Allocator.Temp))
                    {
                        for (var k = 0; k < batches; k++)
                        {
                            var id = cached.Identities[runningTotal];
                            var vertexCount = cached.VertexCounts[runningTotal];
                            var vertexOffset = cached.VertexOffsets[runningTotal];
                            var triangleOffset = cached.TriangleOffsets[runningTotal];
                            var triangleCount = cached.TriangleCounts[runningTotal];


                            verts.AddRange(cached.DNMC.Vertexes.AsArray().GetSubArray(vertexOffset, vertexCount));
                            indexes.AddRange(cached.DNMC.Indexes.AsArray().GetSubArray(triangleOffset, triangleCount));

                            var mesh = UnivoxRenderingJobs.CreateMesh(cached.DNMC.ToDeferred(), vertexOffset,
                                vertexCount, triangleOffset, triangleCount);
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

                        var collider = MeshCollider.Create(verts, indexes);
                        EntityManager.SetComponentData(entity, new PhysicsCollider() {Value = collider});
                    }

                    cached.Dispose();
                }
            }
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


        JobHandle RenderPass(JobHandle handle)
        {
            using (var chunkArray = _renderQuery.CreateArchetypeChunkArray(Allocator.TempJob))
            {
//                var chunkIdType = GetArchetypeChunkComponentType<ChunkIdComponent>(true);
//                var systemEntityVersionType = GetArchetypeChunkComponentType<EntityVersion>();
//
//                var materialVersionType = GetArchetypeChunkComponentType<BlockMaterialIdentityComponent.Version>(true);
//                var subMaterialVersionType =
//                    GetArchetypeChunkComponentType<BlockSubMaterialIdentityComponent.Version>(true);
//                var blockShapeVersionType = GetArchetypeChunkComponentType<BlockShapeComponent.Version>(true);
//                var culledFaceVersionType = GetArchetypeChunkComponentType<BlockCulledFacesComponent.Version>(true);

                Profiler.BeginSample("Process ECS Chunk");
                foreach (var ecsChunk in chunkArray)
                {
//                    var ids = ecsChunk.GetNativeArray(chunkIdType);
//                    var systemVersions = ecsChunk.GetNativeArray(systemEntityVersionType);

                    handle = GenerateBoxelMeshes(ecsChunk, handle);

//                    var materialVersions = ecsChunk.GetNativeArray(materialVersionType);
//                    var subMaterialVersions = ecsChunk.GetNativeArray(subMaterialVersionType);
//                    var blockShapeVersions = ecsChunk.GetNativeArray(blockShapeVersionType);
//                    var culledFaceVersions = ecsChunk.GetNativeArray(culledFaceVersionType);
//                    var voxelChunkEntityArray = ecsChunk.GetNativeArray(chunkArchetype);


//                    var i = 0;
//                    foreach (var voxelChunkEntity in voxelChunkEntityArray)
//                    {
//                        var version = systemVersions[i];
//                        var currentVersion = new EntityVersion()
//                        {
//                            Material = materialVersions[i],
//                            SubMaterial = subMaterialVersions[i],
//                            BlockShape = blockShapeVersions[i],
//                            CulledFaces = culledFaceVersions[i]
//                        };
//                    var matVersion = 
//                    var subMatVersion = 


//                        if (currentVersion.DidChange(version))
//                        {
//                            Profiler.BeginSample("Process Render Chunk");
//                            var results = GenerateBoxelMeshes(voxelChunkEntity, ids[i]);
//                            outHandles = JobHandle.CombineDependencies(outHandles, results);
//                            Profiler.EndSample();
//                            systemVersions[i] = currentVersion;
//                        }


//                        i++;
                }
//                }


                Profiler.EndSample();
            }

            return handle;

//            JobHandle outHandles = new JobHandle();
////
////            var chunkArchetype = GetArchetypeChunkEntityType();
////
////            chunkArray.Dispose();
//
//            return outHandles;
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


        JobHandle GenerateBoxelMeshes(ArchetypeChunk chunk, JobHandle inputDependencies)
        {
            var materialLookup = GetBufferFromEntity<BlockMaterialIdentityComponent>(true);
            var subMaterialLookup = GetBufferFromEntity<BlockSubMaterialIdentityComponent>(true);
            var blockShapeLookup = GetBufferFromEntity<BlockShapeComponent>(true);
            var culledFaceLookup = GetBufferFromEntity<BlockCulledFacesComponent>(true);

            var entityType = GetArchetypeChunkEntityType();

            const int maxBatchSize = Byte.MaxValue;

            //Gather unique materials, these become our batch groups
            var uniqueBatchData = new NativeList<BlockMaterialIdentityComponent>(Allocator.TempJob);
            var uniqueBatchDataCount = new NativeList<int>(Allocator.TempJob);
            var uniqueBatchDataOffset = new NativeList<int>(Allocator.TempJob);
            var createBatches = new FindAndCountUniquePerChunkInBuffer<BlockMaterialIdentityComponent>()
            {
                Unique = uniqueBatchData,
                Chunk = chunk,
                Count = uniqueBatchDataCount,
                EntityType = entityType,
                GetBuffer = materialLookup,
                Offset = uniqueBatchDataOffset
            }.Schedule(inputDependencies);

            //TODO

            var ignore = new NativeArray<bool>(chunk.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var currentVersions = new NativeArray<EntityVersion>(chunk.Count, Allocator.TempJob,
                NativeArrayOptions.UninitializedMemory);
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
            }.Schedule(createBatches);

            var gatherIgnoreJob = new GatherDirtyVersionJob<EntityVersion>()
            {
                Chunk = chunk,
                VersionsType = GetArchetypeChunkComponentType<EntityVersion>(),
                CurrentVersions = currentVersions,
                Ignore = ignore,
            }.Schedule(gatherVersionJob);
            var disposeCurrentVersions =
                new DisposeArrayJob<EntityVersion>(currentVersions).Schedule(gatherIgnoreJob);

//            return disposeCurrentVersions;


            var data = new NativeList<PlanarData>(Allocator.TempJob);
            var dataCount = new NativeList<int>(Allocator.TempJob);
            var dataOffsets = new NativeList<int>(Allocator.TempJob);
            var gatherPlanars = new GatherPlanarJobV2()
            {
                Chunk = chunk,
                CulledFaces = culledFaceLookup,
                Data = data,
                DataCount = dataCount, DataOffsets = dataOffsets,
                Materials = materialLookup,
                EntityType = entityType,
                Shapes = blockShapeLookup,
                SkipEntity = ignore,
                SubMaterials = subMaterialLookup,
                UniqueBatchCounts = uniqueBatchDataCount,
                UniqueBatchOffsets = uniqueBatchDataOffset,
                UniqueBatchValues = uniqueBatchData,
            }.Schedule(disposeCurrentVersions);


            var dynamicNativeMesh = new UnivoxRenderingJobs.DynamicNativeMeshContainer(0, 0, Allocator.TempJob);
            var vertexOffsets = new NativeList<int>(Allocator.TempJob);
            var vertexCounts = new NativeList<int>(Allocator.TempJob);
            var trianglesOffsets = new NativeList<int>(Allocator.TempJob);
            var trianglesCounts = new NativeList<int>(Allocator.TempJob);

            var genJob = new GenerateCubeBoxelMeshJobV2()
            {
                BatchCount = uniqueBatchDataCount.AsArray(),
                DataCounts = dataCount,
                DataOffsets = dataOffsets,
                Ignore = ignore,
                Indexes = dynamicNativeMesh.Indexes,
                NativeCube = new NativeCubeBuilder(Allocator.TempJob),
                Normals = dynamicNativeMesh.Normals,
                Offset = new float3(1f / 2f),
                PlanarData = data.AsDeferredJobArray(),
                Tangents = dynamicNativeMesh.Tangents,
                TextureMap0 = dynamicNativeMesh.TextureMap0,
                TriangleOffsets = trianglesOffsets,
                TriangleSizes = trianglesCounts,
                Vertexes = dynamicNativeMesh.Vertexes,
                VertexSizes = vertexCounts,
                VertexOffsets = vertexOffsets,
            }.Schedule(gatherPlanars);

            //TODO
            var batchGroupIds = new NativeList<BatchGroupIdentity>(Allocator.TempJob);

            var collectBatchGroupIds = new GatherBatchIds()
            {
                BatchIds = batchGroupIds,
                Chunk = chunk,
                ChunkIdComponentType = GetArchetypeChunkComponentType<ChunkIdComponent>(),
                Ignore = ignore,
                UniqueMaterialCounts = uniqueBatchDataCount,
                UniqueMaterials = uniqueBatchData,
                UniqueMaterialOffsets = uniqueBatchDataOffset
            }.Schedule(gatherPlanars);

            JobHandle disposalJobDepend = collectBatchGroupIds;
            disposalJobDepend =
                new DisposeArrayJob<BlockMaterialIdentityComponent>(uniqueBatchData).Schedule(disposalJobDepend);
//            dipsosalJobDepend = new DisposeArrayJob<int>(uniqueBatchDataCount).Schedule(dipsosalJobDepend);
            disposalJobDepend = new DisposeArrayJob<int>(uniqueBatchDataOffset).Schedule(disposalJobDepend);
            disposalJobDepend = new DisposeArrayJob<PlanarData>(data).Schedule(disposalJobDepend);
            disposalJobDepend = new DisposeArrayJob<int>(dataCount).Schedule(disposalJobDepend);
            disposalJobDepend = new DisposeArrayJob<int>(dataOffsets).Schedule(disposalJobDepend);
//            dipsosalJobDepend = new DisposeArrayJob<bool>(ignore).Schedule(dipsosalJobDepend);


            var entityCache =
                new NativeArray<Entity>(chunk.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            entityCache.CopyFrom(chunk.GetNativeArray(entityType));

            var cached = new MultiJobCache()
            {
                BatchCounts = uniqueBatchDataCount,
                DNMC = dynamicNativeMesh,
                Entities = entityCache,
                Identities = batchGroupIds,
                Ignores = ignore,
                TriangleCounts = trianglesCounts,
                TriangleOffsets = trianglesOffsets,
                VertexCounts = vertexCounts,
                VertexOffsets = vertexOffsets,
                WaitHandle = disposalJobDepend
            };

            _jobCache.Enqueue(cached);

//            var dynamicNativeMesh = new UnivoxRenderingJobs.DynamicNativeMeshContainer(0, 0, Allocator.TempJob);
//            var vertexOffsets = new NativeList<int>(Allocator.TempJob);
//            var vertexCounts = new NativeList<int>(Allocator.TempJob);
//            var trianglesOffsets = new NativeList<int>(Allocator.TempJob);
//            var trianglesCounts = new NativeList<int>(Allocator.TempJob);
            return disposalJobDepend;
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
//            inputDeps.Complete();


            CleanupPass();
            SetupPass();

            ProcessJobCache();

            return RenderPass(inputDeps);

//            return new JobHandle();
        }
    }

    internal struct GatherBatchIds : IJob
    {
        public ArchetypeChunk Chunk;
        public ArchetypeChunkComponentType<ChunkIdComponent> ChunkIdComponentType;
        public NativeList<BlockMaterialIdentityComponent> UniqueMaterials;
        public NativeList<int> UniqueMaterialCounts;
        public NativeList<int> UniqueMaterialOffsets;
        public NativeArray<bool> Ignore;
        public NativeList<BatchGroupIdentity> BatchIds;


        public void Execute()
        {
            var ids = Chunk.GetNativeArray(ChunkIdComponentType);
            for (var i = 0; i < Chunk.Count; i++)
            {
                var umc = UniqueMaterialCounts[i];
                if (Ignore[i])
                {
//                    offset += umc;
                    continue;
                }

                for (var j = 0; j < umc; j++)
                {
                    var id = new BatchGroupIdentity()
                    {
                        Chunk = ids[i],
                        MaterialIdentity = UniqueMaterials[UniqueMaterialOffsets[i] + j]
                    };
                    BatchIds.Add(id);
                }
            }
        }
    }
}