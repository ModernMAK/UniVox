using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Jobs
{
    [BurstCompile]
    public struct DeallocateNativeArrayJob<T> : IJob where T : struct
    {
        public DeallocateNativeArrayJob(NativeArray<T> array)
        {
            ArrayToDeallocate = array;
        }

        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<T> ArrayToDeallocate;

        public void Execute()
        {
            //Do nothing
        }
    }
}