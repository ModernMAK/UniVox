using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Jobs
{
    [BurstCompile]
    public struct CreateBatchChunk : IJob
    {
        public CreateBatchChunk(NativeArraySharedValues<int> values) : this(values.GetSharedValueIndexCountArray(),
            values.GetSortedIndices(), values.SharedValueCount)
        {
        }

        public CreateBatchChunk(NativeArray<int> uniqueOffsets, NativeArray<int> sorted, int count)
        {
            BatchIds = new NativeArray<int>(sorted.Length, Allocator.TempJob);
            UniqueOffsets = uniqueOffsets;
            Count = count;
            Sorted = sorted;
        }

        [WriteOnly] public NativeArray<int> BatchIds;
        [ReadOnly] public NativeArray<int> UniqueOffsets;

//        public NativeArray<int> UniqueOffsets;
        [ReadOnly] public NativeArray<int> Sorted;

        [ReadOnly] public int Count;


        public void Execute()
        {
            var runningOffset = 0;
            for (var i = 0; i < Count; i++)
            {
                var len = UniqueOffsets[i];
                for (var j = 0; j < len; j++)
                    BatchIds[Sorted[runningOffset + j]] = i;
                runningOffset += len;
            }
        }
    }
}