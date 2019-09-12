using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace UnityEdits.Rendering
{
    
    //Oookay... We are getting 252 draw calls per mesh... So that's not normal
    //Instead of using combine instance i think i need to make my own mesh combiner, so.... Yay...



    [DisableAutoCreation]
    public class VoxelMeshSystemV3 : JobComponentSystem
    {
        private class MergedRenderUtil
        {
            public MergedRenderUtil()
            {
                CombineInstances = new List<CombineInstance>();
            }

            public Material Material;
            public Mesh Mesh;

            private List<CombineInstance> CombineInstances;
            public int GetCombinedMeshSize() => CombineInstances.Count * (Mesh != null ? Mesh.vertexCount : 0);
            public bool IsMeshOverCapacity() => GetCombinedMeshSize() > ushort.MaxValue;

            public void AddCapacity(int count)
            {
                CombineInstances.Capacity = math.max(CombineInstances.Capacity, CombineInstances.Count + count);
            }

            public void AddCombiner(CombineInstance ci) => CombineInstances.Add(ci);

            public void AddCombiner(NativeArray<float4x4> matrixes)
            {
                AddCapacity(matrixes.Length);
                for (var i = 0; i < matrixes.Length; i++)
                    AddCombiner(new CombineInstance()
                    {
                        mesh = Mesh,
                        transform = matrixes[i],
                    });
            }

            public void ClearCombiner() => CombineInstances.Clear();


            public RenderMesh UpdateRenderMesh(RenderMesh renderMesh)
            {
                MergeIntoMesh(renderMesh.mesh);
                renderMesh.material = Material;
                return renderMesh;
            }

            public void MergeIntoMesh(Mesh target)
            {
                target.Clear();
                if (Mesh != null && CombineInstances.Count > 0)
                    target.CombineMeshes(CombineInstances.ToArray(), true, true);
            }

            public void Clear()
            {
                ClearCombiner();
                Material = null;
                Mesh = null;
            }
        }

        [BurstCompile]
        struct GatherSharedComponentIndexes<T> : IJobParallelFor where T : struct, ISharedComponentData
        {
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly] public ArchetypeChunkSharedComponentType<T> ComponentType;
            [WriteOnly] public NativeArray<int> Results;

            public void Execute(int index)
            {
                Results[index] = Chunks[index].GetSharedComponentIndex<T>(ComponentType);
            }

            public static GatherSharedComponentIndexes<T> Create(NativeArray<ArchetypeChunk> chunks,
                ArchetypeChunkSharedComponentType<T> componentType, NativeArray<int> results)
            {
                return new GatherSharedComponentIndexes<T>()
                {
                    Chunks = chunks,
                    Results = results,
                    ComponentType = componentType
                };
            }

            public static GatherSharedComponentIndexes<T> Create(NativeArray<ArchetypeChunk> chunks,
                ArchetypeChunkSharedComponentType<T> componentType, out NativeArray<int> results)
            {
                results = new NativeArray<int>(chunks.Length, Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);
                return Create(chunks, componentType, results);
            }
        }

        struct MergedData : IEquatable<MergedData>
        {
            public int MaterialId;
            public int MeshId;

            public int3 ChunkPosition;
            public int3 WorldPos => ChunkPosition * ChunkSize.AxisSize;

            public int MeshSize;

            public static MergedData Create(VoxelRenderData vrd, ChunkPosition cPos, int meshSize)
            {
                return new MergedData()
                {
                    MaterialId = vrd.MaterialIdentity,
                    MeshId = vrd.MeshIdentity,
                    ChunkPosition = cPos.Position,
                    MeshSize = meshSize
                };
            }

            public bool Equals(MergedData other)
            {
                return MaterialId == other.MaterialId && MeshId == other.MeshId &&
                       ChunkPosition.Equals(other.ChunkPosition) && MeshSize == other.MeshSize;
            }

            public override bool Equals(object obj)
            {
                return obj is MergedData other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = MaterialId;
                    hashCode = (hashCode * 397) ^ MeshId;
                    hashCode = (hashCode * 397) ^ ChunkPosition.GetHashCode();
                    hashCode = (hashCode * 397) ^ MeshSize;
                    return hashCode;
                }
            }
        }


        private const int batchCount = 255;

        JobHandle GatherSharedComponentIndexesJob<T>(NativeArray<ArchetypeChunk> chunks,
            ArchetypeChunkSharedComponentType<T> componentType, out NativeArray<int> results,
            JobHandle inputDeps = default) where T : struct, ISharedComponentData
        {
            var chunksCount = chunks.Length;

            var gatherJob = GatherSharedComponentIndexes<T>.Create(chunks, componentType, out results);

            return gatherJob.Schedule(chunksCount, batchCount, inputDeps);
        }


        JobHandle SortSharedComponentIndexesJob<T>(NativeArray<T> sharedIds, out NativeArraySharedValues<T> results,
            JobHandle inputDeps = default) where T : struct, IComparable<T>
        {
            results = new NativeArraySharedValues<T>(sharedIds, Allocator.TempJob);

            return results.Schedule(inputDeps);
        }

        public struct UniqueSharedComponentIndexesJob<T> : IJob where T : struct
        {
            [ReadOnly] public NativeArray<int> SortedIndexes;
            [ReadOnly] public NativeArray<int> IndexLengths;
            [ReadOnly] public NativeArray<T> Buffer;
            [WriteOnly] public NativeArray<T> Results;

            public void Execute()
            {
                var trueIndex = 0;
                for (var i = 0; i < IndexLengths.Length; i++)
                {
                    Results[i] = Buffer[SortedIndexes[trueIndex]];
                    trueIndex += IndexLengths[i];
                }
            }
        }

        JobHandle GetUniqueSharedComponentIndexesJob<T>(NativeArraySharedValues<T> sharedValues,
            out NativeArray<T> results, JobHandle inputDeps = default) where T : struct, IComparable<T>
        {
            var uniqueCount = sharedValues.SharedValueCount;
            results = new NativeArray<T>(uniqueCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var buffer = sharedValues.SourceBuffer;
            var sorted = sharedValues.GetSortedIndices();
            var lengths = sharedValues.GetSharedValueIndexCountArray();


            var job = new UniqueSharedComponentIndexesJob<T>()
            {
                SortedIndexes = sorted,
                IndexLengths = lengths,
                Buffer = buffer,
                Results = results
            };

            return job.Schedule(inputDeps);
        }

//
//        struct MergeNativeArrayJob<T> : IJobParallelFor where T : struct
//        {
//            [ReadOnly] public NativeArray<T> First;
//            [ReadOnly] public NativeArray<T> Second;
//            [WriteOnly] public NativeArray<T> Result;
//
//
//            public void Execute(int index)
//            {
//                var fLen = First.Length;
//                if (index > fLen)
//                    Result[index] = Second[index - fLen];
//                else
//                    Result[index] = First[index];
//            }
//
//            public static MergeNativeArrayJob<T> Create(NativeArray<T> first, NativeArray<T> second,
//                out NativeArray<T> result)
//            {
//                result = new NativeArray<T>(first.Length + second.Length, Allocator.TempJob,
//                    NativeArrayOptions.UninitializedMemory);
//                return new MergeNativeArrayJob<T>()
//                {
//                    First = first,
//                    Second = second,
//                    Result = result
//                };
//            }
//        }
//
//        struct MergeNativeArrayJobDeallocateFirst<T> : IJobParallelFor where T : struct
//        {
//            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<T> First;
//            [ReadOnly] public NativeArray<T> Second;
//            [WriteOnly] public NativeArray<T> Result;
//
//
//            public void Execute(int index)
//            {
//                var fLen = First.Length;
//                if (index > fLen)
//                    Result[index] = Second[index - fLen];
//                else
//                    Result[index] = First[index];
//            }
//
//            public static MergeNativeArrayJob<T> Create(NativeArray<T> first, NativeArray<T> second,
//                out NativeArray<T> result)
//            {
//                result = new NativeArray<T>(first.Length + second.Length, Allocator.TempJob,
//                    NativeArrayOptions.UninitializedMemory);
//                return new MergeNativeArrayJob<T>()
//                {
//                    First = first,
//                    Second = second,
//                    Result = result
//                };
//            }
//        }
//
//        struct MergeNativeArrayJobDeallocateAll<T> : IJobParallelFor where T : struct
//        {
//            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<T> First;
//            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<T> Second;
//            [WriteOnly] public NativeArray<T> Result;
//
//
//            public void Execute(int index)
//            {
//                var fLen = First.Length;
//                if (index > fLen)
//                    Result[index] = Second[index - fLen];
//                else
//                    Result[index] = First[index];
//            }
//
//            public static MergeNativeArrayJob<T> Create(NativeArray<T> first, NativeArray<T> second,
//                out NativeArray<T> result)
//            {
//                result = new NativeArray<T>(first.Length + second.Length, Allocator.TempJob,
//                    NativeArrayOptions.UninitializedMemory);
//                return new MergeNativeArrayJob<T>()
//                {
//                    First = first,
//                    Second = second,
//                    Result = result
//                };
//            }
//        }
//
//        JobHandle MergeNativeArrays<T>(out NativeArray<T> result, JobHandle inputDeps = default,
//            params NativeArray<T>[] array) where T : struct, IEquatable<T>
//        {
//            if (array.Length < 2)
//                throw new Exception();
//
//
//            var job = MergeNativeArrayJob<T>.Create(array[0], array[1], out result)
//                .Schedule(result.Length, batchCount, inputDeps);
//
//            for (var i = 2; i < array.Length; i++)
//            {
//                job = MergeNativeArrayJobDeallocateFirst<T>.Create(result, array[i], out result)
//                    .Schedule(result.Length, batchCount, job);
//            }
//
//            return job;
//        }
//
//        JobHandle MergeNativeArraysDeallocateAll<T>(out NativeArray<T> result, JobHandle inputDeps = default,
//            params NativeArray<T>[] array) where T : struct, IEquatable<T>
//        {
//            if (array.Length < 2)
//                throw new Exception();
//
//
//            var job = MergeNativeArrayJobDeallocateAll<T>.Create(array[0], array[1], out result)
//                .Schedule(result.Length, batchCount, inputDeps);
//
//            for (var i = 2; i < array.Length; i++)
//            {
//                job = MergeNativeArrayJobDeallocateAll<T>.Create(result, array[i], out result)
//                    .Schedule(result.Length, batchCount, job);
//            }
//
//            return job;
//        }

        JobHandle GatherChunkMatrix(ArchetypeChunk chunk, ArchetypeChunkComponentType<LocalToWorld> componentType,
            out NativeArray<float4x4> matrix, float3 matrixOffset, JobHandle inputDeps = default)
        {
            var chunksCount = chunk.Count;

            var gatherJob = new GatherVoxelRenderMatrixV2()
            {
                Matricies = matrix = new NativeArray<float4x4>(chunksCount, Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory),
                LocalToWorlds = chunk.GetNativeArray(componentType),
                MatrixOffset = matrixOffset
            };

            return gatherJob.Schedule(chunksCount, batchCount, inputDeps);
        }


//        
//        JobHandle GatherChunkMatrix(ArchetypeChunk chunk, out NativeArray<float4x4> matrix,
//            float3 matrixOffset, JobHandle inputDeps = default)
//        {
//            var chunksCount = chunk.Count;
//
//            var gatherJob = new GatherVoxelRenderMatrixV2()
//            {
//                Matricies = matrix = new NativeArray<float4x4>(chunksCount, Allocator.TempJob,
//                    NativeArrayOptions.UninitializedMemory),
//                LocalToWorlds = chunk.GetNativeArray(GetArchetypeChunkComponentType<LocalToWorld>(true)),
//                MatrixOffset = matrixOffset
//            };
//
//            return gatherJob.Schedule(chunksCount, batchCount, inputDeps);
//        }

        public void ClearUpdateCache()
        {
            ChunkPositionCache.Clear();
            foreach (var value in ChunkCombinerCache.Values)
            {
                _combinerPool.Enqueue(value);
                value.Clear();
            }

            ChunkCombinerCache.Clear();
        }

        private Dictionary<int, int3> ChunkPositionCache;
        private Dictionary<MergedData, MergedRenderUtil> ChunkCombinerCache;
        private Dictionary<int3, Entity> ChunkEntityCache;
        private Dictionary<ArchetypeChunk, MergedData> ArchetypeCache;

        private EntityQuery CombineMeshQuery;
        private EntityQuery SetupChunkComponentQuery;
        private EntityQuery CleanupChunkComponentQuery;
        private EntityArchetype EntityChunkArchetype;
        public MasterRegistry MasterRegistry => GameManager.MasterRegistry;

        protected override void OnCreate()
        {
            ChunkPositionCache = new Dictionary<int, int3>();
            ArchetypeCache = new Dictionary<ArchetypeChunk, MergedData>();

            ChunkCombinerCache = new Dictionary<MergedData, MergedRenderUtil>();

            CombineMeshQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VoxelRenderData>(),
                    ComponentType.ReadOnly<ChunkPosition>(),
                    ComponentType.ReadOnly<LocalToWorld>(),
                    ComponentType.ChunkComponentReadOnly<ChunkEntity>(),
                },
                None = new[]
                {
                    ComponentType.ReadOnly<DontRenderTag>(),
                },
                Options = EntityQueryOptions.Default
            });

            CombineMeshQuery.SetFilterChanged(new ComponentType[]
            {
                typeof(VoxelRenderData),
                typeof(ChunkPosition)
            });

            SetupChunkComponentQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<VoxelRenderData>(),
                    ComponentType.ReadOnly<ChunkPosition>(),
                    ComponentType.ReadOnly<LocalToWorld>(),
                },
                None = new[]
                {
                    ComponentType.ChunkComponentReadOnly<ChunkEntity>(),
                    ComponentType.ReadOnly<DontRenderTag>(),
                },
                Options = EntityQueryOptions.Default
            });
            CleanupChunkComponentQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ChunkComponentReadOnly<ChunkEntity>(),
                },
                None = new[]
                {
                    ComponentType.ReadOnly<VoxelRenderData>(),
                    ComponentType.ReadOnly<ChunkPosition>(),
                    ComponentType.ReadOnly<LocalToWorld>(),
                },
                Options = EntityQueryOptions.Default
            });


//            ComponentType.ChunkComponentReadOnly<ChunkWorldRenderBounds>(),
//            ComponentType.ReadOnly<RootLodRequirement>(), ComponentType.ReadOnly<LodRequirement>(), => MeshLODComponent
//            ComponentType.ReadOnly<WorldRenderBounds>(),
//            ComponentType.ReadOnly<RenderMesh>(),
            EntityChunkArchetype = EntityManager.CreateArchetype(
//                typeof(MeshLODComponent), TODO fix this
                typeof(WorldRenderBounds),
                typeof(RenderMesh),
                ComponentType.ChunkComponent<ChunkWorldRenderBounds>(),
                typeof(Translation),
                typeof(Rotation),
                typeof(LocalToWorld)
            );

            _combinerPool = new Queue<MergedRenderUtil>();
        }

        public int3 GetPosition(int componentIndex)
        {
            Profiler.BeginSample("Fetch Chunk Position");
            if (!ChunkPositionCache.TryGetValue(componentIndex, out var result))
            {
                result = ChunkPositionCache[componentIndex] =
                    EntityManager.GetSharedComponentData<ChunkPosition>(componentIndex).Position;
            }

            Profiler.EndSample();
            return result;
        }


        void AddCombinersPerChunk(MergedData chunkPosIndex, NativeArray<float4x4> matrixes)
        {
            var list = ChunkCombinerCache[chunkPosIndex];
            list.AddCapacity(matrixes.Length);
            for (var i = 0; i < matrixes.Length; i++)
            {
                var ci = new CombineInstance()
                {
                    transform = matrixes[i],
                    mesh = list.Mesh
                };
                list.AddCombiner(ci);
            }
        }


        void GatherSortedIds<T>(NativeArray<ArchetypeChunk> chunks, ArchetypeChunkSharedComponentType<T> componentType,
            out NativeArray<int> results, out NativeArraySharedValues<int> sharedResults, JobHandle inputDeps = default)
            where T : struct, ISharedComponentData
        {
            inputDeps.Complete();
//            var gatherJobs = new NativeArray<JobHandle>(chunks.Length, Allocator.TempJob);

            var gatherJob = GatherSharedComponentIndexesJob(chunks, componentType, out var chunkResults);

            //WOW I AM DUMB - Alot of extra work that slows us down because we do the same thing N times! Fixe withe the above line
//            var chunkedResults = new NativeArray<int>[chunks.Length];
//            
//            
//            
//            for (var i = 0; i < chunks.Length; i++)
//            {
//                gatherJobs[i] = GatherSharedComponentIndexesJob(chunks, componentType, out var chunkResults);
//                chunkedResults[i] = chunkResults;
//            }
//
//            var gatherHandle = JobHandle.CombineDependencies(gatherJobs);


            //This cleans up our chunkedResults, everything except for our final result is deallocated
//            var mergeJob = MergeNativeArraysDeallocateAll(out var mergedResults, gatherHandle, chunkedResults);


            var sortedJob = SortSharedComponentIndexesJob(chunkResults, out var sharedChunkResults, gatherJob);
            sortedJob.Complete();
//            gatherJobs.Dispose();


            results = chunkResults;
            sharedResults = sharedChunkResults;
        }

        void GatherAllUniqueIds<T>(NativeArray<ArchetypeChunk> chunks,
            ArchetypeChunkSharedComponentType<T> componentType, out NativeArray<int> results,
            JobHandle inputDeps = default) where T : struct, ISharedComponentData
        {
            inputDeps.Complete();
            GatherSortedIds(chunks, componentType, out var mergedResults, out var sharedResults);

            //WE no longer need sorted or merge, deallocate them
            var uniqueJob = GetUniqueSharedComponentIndexesJob(sharedResults, out results);

            //WE need to copmlete uniqueJob before we can dispose
            uniqueJob.Complete();

            mergedResults.Dispose();
            sharedResults.Dispose();
        }


        void GatherAllUniqueIds(NativeArraySharedValues<int> values, out NativeArray<int> results,
            JobHandle inputDeps = default)
        {
            inputDeps.Complete();

            //WE no longer need sorted or merge, deallocate them
            var uniqueJob = GetUniqueSharedComponentIndexesJob(values, out results);

            //WE need to copmlete uniqueJob before we can dispose
            uniqueJob.Complete();
        }


        void GatherSharedComponentDataValues<T>(NativeArray<int> indexes, out NativeArray<T> results)
            where T : struct, ISharedComponentData
        {
            results = new NativeArray<T>(indexes.Length, Allocator.TempJob);
            for (var i = 0; i < indexes.Length; i++)
                results[i] = EntityManager.GetSharedComponentData<T>(indexes[i]);
        }

        void GatherSharedComponentDataValueLookup<T>(NativeArray<int> indexes, out NativeHashMap<int, T> results)
            where T : struct, ISharedComponentData
        {
            results = new NativeHashMap<int, T>(indexes.Length, Allocator.TempJob);
            for (var i = 0; i < indexes.Length; i++)
            {
                var index = indexes[i];
                results[index] = EntityManager.GetSharedComponentData<T>(index);
            }
        }

        void SetupUtil(NativeArray<ArchetypeChunk> chunks)
        {
            ClearUpdateCache();

            var renderDataType = GetArchetypeChunkSharedComponentType<VoxelRenderData>();
            var chunkPosType = GetArchetypeChunkSharedComponentType<ChunkPosition>();

            GatherSortedIds(chunks, renderDataType, out var renderIds, out var sharedRenderIds);
            GatherAllUniqueIds(sharedRenderIds, out var uniqueRenderIds);
            GatherSharedComponentDataValueLookup<VoxelRenderData>(uniqueRenderIds, out var renderDataTable);


            GatherSortedIds(chunks, chunkPosType, out var chunkPosIds, out var sharedChunkPosIds);
            GatherAllUniqueIds(sharedChunkPosIds, out var uniqueChunkPosIds);
            GatherSharedComponentDataValueLookup<ChunkPosition>(uniqueChunkPosIds, out var chunkPosTable);


            for (var i = 0; i < chunks.Length; i++)
            {
                var chunk = chunks[i];
                var renderData = renderDataTable[renderIds[i]];
                var chunkPos = chunkPosTable[chunkPosIds[i]];

                MasterRegistry.Mesh.TryGetValue(renderData.MeshIdentity, out var mesh);
                MasterRegistry.Material.TryGetValue(renderData.MaterialIdentity, out var material);

                var mergedData = MergedData.Create(renderData, chunkPos, mesh != null ? mesh.vertexCount : 0);

                var combineUtil = GetCombinerList();
                combineUtil.Mesh = mesh;
                combineUtil.Material = material;

                ArchetypeCache[chunk] = mergedData;
                ChunkCombinerCache[mergedData] = combineUtil;
            }


            renderIds.Dispose();
            sharedRenderIds.Dispose();
            uniqueRenderIds.Dispose();
            renderDataTable.Dispose();

            chunkPosIds.Dispose();
            sharedChunkPosIds.Dispose();
            uniqueChunkPosIds.Dispose();
            chunkPosTable.Dispose();
        }

//        private MergedRenderUtil QuickGetCombinerFromCache(ArchetypeChunk chunk) => ChunkCombinerCache[];

        void AddCombiners(NativeArray<ArchetypeChunk> chunks)
        {
            for (var i = 0; i < chunks.Length; i++)
            {
                AddCombinersPerChunk(chunks[i]);
            }

//
//                materials = new Material[chunks.Length];
//            meshSizes = new int[chunks.Length];
//            //Gather Types to access Data
//            var voxelRenderDataType = GetArchetypeChunkSharedComponentType<VoxelRenderData>();
//            var chunkPositionType = GetArchetypeChunkSharedComponentType<ChunkPosition>();
//
//            //Determine Unique Values
//            var sharedRenderCount = sortedRenderDataIds.SharedValueCount;
//            //Get Array of Count Per Unique Values
//            var sharedRendererCounts = sortedRenderDataIds.GetSharedValueIndexCountArray();
//            //Get Array of 'Grouped' Indexes based on their Unique Value
//            var sortedChunkIndices = sortedRenderDataIds.GetSortedIndices();
//
//
//            Profiler.BeginSample("Process Chunks");
//
//            //We have to keep an offset of what we have inspected
//            var sortedChunkIndex = 0;
//            for (var i = 0; i < sharedRenderCount; i++)
//            {
//                //The Range of the Sorted Chunk Indeces
//                //These chunks share a MESH and MATERIAL
//                var startSortedChunkIndex = sortedChunkIndex;
//                var endSortedChunkIndex = startSortedChunkIndex + sharedRendererCounts[i];
//
//
//                Profiler.BeginSample("Gather Template");
//
//                var templateChunk = chunks[sortedChunkIndices[sortedChunkIndex]];
//
//                var voxelRenderDataSharedComponentIndex =
//                    templateChunk.GetSharedComponentIndex(voxelRenderDataType);
//
//                var voxelRenderData =
//                    EntityManager.GetSharedComponentData<VoxelRenderData>(voxelRenderDataSharedComponentIndex);
//
//                var templateMeshFound =
//                    MasterRegistry.Mesh.TryGetValue(voxelRenderData.MeshIdentity, out var templateMesh);
//                var templateMaterialFound =
//                    MasterRegistry.Material.TryGetValue(voxelRenderData.MaterialIdentity, out var templateMaterial);
//
//
//                Profiler.EndSample();
//
//                Profiler.BeginSample("Process Batch");
//                //For loop without a for
//                while (sortedChunkIndex < endSortedChunkIndex)
//                {
//                    //Get the index from our sorted indexes
//                    var chunkIndex = sortedChunkIndices[sortedChunkIndex];
//
//                    materials[chunkIndex] = templateMaterial;
//
//                    //If templateMesh Fails, we skip the batch
//                    //We could also include failing to find mat, but i believe that is not fatal
//                    if (!templateMeshFound)
//                    {
//                        meshSizes[chunkIndex] = 0;
//                        continue;
//                    }
//
//                    meshSizes[chunkIndex] = 0;
//
//                    //Get the chunk
//                    var chunk = chunks[chunkIndex];
//                    var chunkCount = chunk.Count;
//
//                    //Get the index to the Render Mesh from our chunk
////                        var voxelRenderDataSharedComponentIndex = chunk;
//                    var chunkPositionSharedComponentIndex = chunk.GetSharedComponentIndex(chunkPositionType);
//
//                    Profiler.BeginSample("Gather Matrixes");
//                    var gatheredMatrix = GatherChunkMatrix(chunk, out var matrixes,
//                        GetPosition(chunkPositionSharedComponentIndex));
//                    gatheredMatrix.Complete();
//                    Profiler.EndSample();
//
//
//                    ChunkCombinerCache[chunkPositionSharedComponentIndex] = GetCombinerList();
//                    Profiler.BeginSample("Create Combiner");
//                    AddCombinersPerChunk(chunkPositionSharedComponentIndex, matrixes, templateMesh);
//                    Profiler.EndSample();
//
//                    matrixes.Dispose();
//
//                    sortedChunkIndex++;
//                }
//
//                Profiler.EndSample();
//            }
//
//            Profiler.EndSample();
        }

        private void AddCombinersPerChunk(ArchetypeChunk chunk)
        {
            var chunkPosIndex = ArchetypeCache[chunk];
            var gatherMatrixes = GatherChunkMatrix(chunk, GetArchetypeChunkComponentType<LocalToWorld>(true),
                out var matrix, chunkPosIndex.WorldPos);

            gatherMatrixes.Complete();

            var combiner = this.ChunkCombinerCache[chunkPosIndex];

            combiner.AddCombiner(matrix);
            matrix.Dispose();
        }

        private Queue<MergedRenderUtil> _combinerPool;

        private MergedRenderUtil GetCombinerList()
        {
            return _combinerPool.Count > 0 ? _combinerPool.Dequeue() : new MergedRenderUtil();
        }

        void UpdateMesh(ArchetypeChunk chunk)
        {
            var key = ArchetypeCache[chunk];
            var combiner = ChunkCombinerCache[key];

            var chunkEntity = chunk.GetChunkComponentData(GetArchetypeChunkComponentType<ChunkEntity>()).Entity;
            var renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(chunkEntity);


            renderMesh = combiner.UpdateRenderMesh(renderMesh);

            EntityManager.SetSharedComponentData(chunkEntity, renderMesh);
        }

        void UpdateMeshes(NativeArray<ArchetypeChunk> chunks)
        {
            for (var i = 0; i < chunks.Length; i++)
                UpdateMesh(chunks[i]);
        }

        void UpdateMesh(ArchetypeChunk chunk, CombineInstance[] combiners, Material material, int meshSize)
        {
            var chunkEntity = chunk.GetChunkComponentData(GetArchetypeChunkComponentType<ChunkEntity>()).Entity;
            var renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(chunkEntity);

            renderMesh.mesh.Clear(true); //TODO test for errors, assume it does not for now


            if (combiners.Length > 0)
            {
                var mergedMeshSize = meshSize * combiners.Length;
                var requiredMeshes = (int) math.ceil((float) mergedMeshSize / ushort.MaxValue);

                if (requiredMeshes > 1)
                    Debug.LogWarning("Mesh is too Big to be combined!");


                renderMesh.mesh.CombineMeshes(combiners, true, true, false);
            }

            renderMesh.material = material;
            EntityManager.SetSharedComponentData(chunkEntity, renderMesh);
        }

        void UpdateMeshes(NativeArray<ArchetypeChunk> chunks, CombineInstance[][] combiners, Material[] materials,
            int[] meshSizes)
        {
            for (var i = 0; i < chunks.Length; i++)

                UpdateMesh(chunks[i], combiners[i], materials[i], meshSizes[i]);
        }

        private void SetupEntityChunk()
        {
            var query = SetupChunkComponentQuery;
            var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob);
            var count = chunks.Length;

            EntityManager.AddChunkComponentData(query, new ChunkEntity());
            for (var i = 0; i < count; i++)
            {
                var chunk = chunks[i];
                var chunkEntity = new ChunkEntity()
                {
                    Entity = EntityManager.CreateEntity(EntityChunkArchetype)
                };
                var renderMesh = new RenderMesh()
                {
                    castShadows = ShadowCastingMode.On,
                    layer = 0,
                    material = null,
                    mesh = new Mesh(),
                    receiveShadows = true,
                    subMesh = 0
                };
                renderMesh.mesh.name = "Combined Render Mesh";
                EntityManager.SetSharedComponentData(chunkEntity.Entity, renderMesh);

                EntityManager.SetChunkComponentData(chunk, chunkEntity);
            }

            chunks.Dispose();
        }

        private void CleanupEntityChunk()
        {
            var query = CleanupChunkComponentQuery;

            EntityManager.RemoveChunkComponentData<ChunkEntity>(query);
        }

        private void UpdateAndCombineMeshes()
        {
            var chunks = CombineMeshQuery.CreateArchetypeChunkArray(Allocator.TempJob);

            //Skip everything if there are no chunks
            if (chunks.Length <= 0)
            {
                chunks.Dispose();
                return;
            }

            Profiler.BeginSample("Create Batches");
            SetupUtil(chunks);
            Profiler.EndSample();

            Profiler.BeginSample("Apply Transform");
            AddCombiners(chunks);
            Profiler.EndSample();

            Profiler.BeginSample("Generate Mesh");
            UpdateMeshes(chunks);
            Profiler.EndSample();
            
            chunks.Dispose();
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete();

            Profiler.BeginSample("Cleanup");
            CleanupEntityChunk();
            Profiler.EndSample();

            Profiler.BeginSample("Setup");
            SetupEntityChunk();
            Profiler.EndSample();

            Profiler.BeginSample("Update / Combine");
            UpdateAndCombineMeshes();
            Profiler.EndSample();

            return new JobHandle();
        }
    }
}