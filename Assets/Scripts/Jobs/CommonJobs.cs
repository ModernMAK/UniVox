using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Jobs
{
    public static class CommonJobs
    {
        public static JobHandle Sort<T>(NativeArray<T> source, out NativeArraySharedValues<T> sharedValues,
            JobHandle dependencies = default)
            where T : struct, IComparable<T>
        {
            sharedValues = new NativeArraySharedValues<T>(source, Allocator.TempJob);
            return sharedValues.Schedule(dependencies);
        }

        public static NativeArraySharedValues<T> Sort<T>(NativeArray<T> source, JobHandle dependencies = default)
            where T : struct, IComparable<T>
        {
            Sort(source, out var sharedValues, dependencies).Complete();
            return sharedValues;
        }

        //Helper function
        public static void GatherUnique<T>(NativeArraySharedValues<T> shared, out int uniqueCount,
            out NativeArray<int> uniqueOffsets, out NativeArray<int> lookupIndexes) where T : struct, IComparable<T>
        {
            uniqueCount = shared.SharedValueCount;
            uniqueOffsets = shared.GetSharedValueIndexCountArray();
            lookupIndexes = shared.GetSortedIndices();
        }

        public static NativeSlice<int> CreateBatch(int batchId, NativeArray<int> uniqueOffsets,
            NativeArray<int> lookupIndexes)
        {
            var start = 0;
            var end = 0;

            for (var i = 0; i <= batchId; i++)
            {
                start = end;
                end += uniqueOffsets[i];
            }


            var slice = new NativeSlice<int>(lookupIndexes, start, end);
            return slice;
        }


        public static NativeSlice<int>[] CreateBatches(int batchCount, NativeArray<int> uniqueOffsets,
            NativeArray<int> lookupIndexes)
        {
            var batches = new NativeSlice<int>[batchCount];
            var start = 0;
            for (var i = 0; i < batchCount; i++)
            {
                var offset = uniqueOffsets[i];
                batches[i] = new NativeSlice<int>(lookupIndexes, start, offset);
                start += uniqueOffsets[i];
            }

            return batches;
        }
    }
}