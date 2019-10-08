using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ECS.UniVox.Systems
{
    [BurstCompile]
    public struct DisposeQueueJob<T> : IJob where T : struct
    {
        public DisposeQueueJob(NativeQueue<T> queue)
        {
            Queue = queue;
        }

        [DeallocateOnJobCompletion] public NativeQueue<T> Queue;

        public void Execute()
        {
            //Do NOTHING
        }
    }
}