using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.System
{
    //OOOHH BOOOY.... ONE MORE TIME

    //SO, we need to...
    //Get Unique Data -> this is a 'Concurrency Point'
    //Convert Shared Array Values to an indipendent Array
    //Convert Shared Array Values to Unique Values (Due to not being able to jobify, this might as well be a concurrency point)
    //Cleanup arrays -> this is a concurrency Point


    public static class GatherUtil
    {
        public static SharedComponentDataArray<TGather> Gather<TGather>(
            NativeArray<ArchetypeChunk> chunks, EntityManager manager, JobHandle inputDeps = default)
            where TGather : struct, ISharedComponentData
        {
            inputDeps.Complete();
            var indexes = GetIndexes<TGather>(chunks, manager);
            var converted = GetConvertedIndexes(indexes);
            var unique = GetUniqueIndexes(indexes);
            var data = GetData<TGather>(unique, manager);

            indexes.SourceBuffer.Dispose();
            indexes.Dispose();
            unique.Dispose();

            return new SharedComponentDataArray<TGather>
            {
                data = data,
                indexes = converted
            };
        }

        public static SharedComponentDataArrayManaged<TGather> GatherManaged<TGather>(
            NativeArray<ArchetypeChunk> chunks, EntityManager manager, JobHandle inputDeps = default)
            where TGather : struct, ISharedComponentData
        {
            inputDeps.Complete();
            var indexes = GetIndexes<TGather>(chunks, manager);
            var converted = GetConvertedIndexes(indexes);
            var unique = GetUniqueIndexes(indexes);
            var data = GetManagedData<TGather>(unique, manager);

            indexes.SourceBuffer.Dispose();
            indexes.Dispose();
            unique.Dispose();

            return new SharedComponentDataArrayManaged<TGather>
            {
                data = data,
                indexes = converted
            };
        }

        public static TGather[] GetManagedData<TGather>(NativeArray<int> sharedIndex, EntityManager manager)
            where TGather : struct, ISharedComponentData
        {
            var uniqueValues = sharedIndex.Length;
            var data = new TGather[uniqueValues];

            for (var index = 0; index < uniqueValues; index++)
                data[index] = manager.GetSharedComponentData<TGather>(sharedIndex[index]);

            return data;
        }

        public static NativeArray<TGather> GetData<TGather>(NativeArray<int> sharedIndex, EntityManager manager)
            where TGather : struct, ISharedComponentData
        {
            var uniqueValues = sharedIndex.Length;
            var data = new NativeArray<TGather>(uniqueValues, Allocator.TempJob,
                NativeArrayOptions.UninitializedMemory);

            for (var index = 0; index < uniqueValues; index++)
                data[index] = manager.GetSharedComponentData<TGather>(sharedIndex[index]);

            return data;
        }

        public static NativeArray<int> GetUniqueIndexes(NativeArraySharedValues<int> sharedIndex)
        {
            var uniqueValues = sharedIndex.SharedValueCount;
            var sortedIndices = sharedIndex.GetSortedIndices();
            var countPerValue = sharedIndex.GetSharedValueIndexCountArray();
            var gatheredValues =
                new NativeArray<int>(uniqueValues, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var source = sharedIndex.SourceBuffer;
            var runningOffset = 0;
            for (var i = 0; i < uniqueValues; i++)
            {
                var sourceIndex = sortedIndices[runningOffset];
                var sharedComponentDataIndex = source[sourceIndex];
                gatheredValues[i] = sharedComponentDataIndex;
                runningOffset += countPerValue[i];
            }

            return gatheredValues;
        }

        public static NativeArray<int> GetConvertedIndexes(NativeArraySharedValues<int> sharedIndex,
            JobHandle inputDeps = default)
        {
            var chunkRenderer =
                new NativeArray<int>(sharedIndex.SourceBuffer.Length, Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory);

            var convertJob = new ConvertJob
            {
                Indexes = sharedIndex,
                Converted = chunkRenderer
            };
            convertJob.Schedule(sharedIndex.SourceBuffer.Length, 64, inputDeps).Complete();
            return chunkRenderer;
        }


        public static NativeArraySharedValues<int> GetIndexes<TGather>(NativeArray<ArchetypeChunk> chunks,
            EntityManager manager, JobHandle inputDeps = default) where TGather : struct, ISharedComponentData
        {
            var chunkRenderer =
                new NativeArray<int>(chunks.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var sortedChunks = new NativeArraySharedValues<int>(chunkRenderer, Allocator.TempJob);

            var gatherDataJob = new GatherIndexes<TGather>
            {
                Chunks = chunks,
                GatherType = manager.GetArchetypeChunkSharedComponentType<TGather>(),
                ChunkGatherIndex = chunkRenderer
            };
            var gatherDataJobHandle = gatherDataJob.Schedule(chunks.Length, 64, inputDeps);
            var sortedChunksJobHandle = sortedChunks.Schedule(gatherDataJobHandle);
            sortedChunksJobHandle.Complete();
            return sortedChunks;
        }

        [BurstCompile]
        private struct GatherIndexes<TComponent> : IJobParallelFor where TComponent : struct, ISharedComponentData
        {
            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
            [ReadOnly] public ArchetypeChunkSharedComponentType<TComponent> GatherType;
            public NativeArray<int> ChunkGatherIndex;

            public void Execute(int chunkIndex)
            {
                var chunk = Chunks[chunkIndex];
                var sharedIndex = chunk.GetSharedComponentIndex(GatherType);
                ChunkGatherIndex[chunkIndex] = sharedIndex;
            }
        }


        private struct ConvertJob : IJobParallelFor
        {
            [ReadOnly] public NativeArraySharedValues<int> Indexes;
            public NativeArray<int> Converted;

            public void Execute(int index)
            {
                Converted[index] = Indexes.GetSharedIndexBySourceIndex(index);
            }
        }
    }
}