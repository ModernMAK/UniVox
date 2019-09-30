using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UniVox.Rendering.ChunkGen.Jobs
{
    [BurstCompile]
    public struct NativeQueueToNativeListJob<T> : IJob where T : struct
    {
        public NativeQueue<T> Queue;
        [WriteOnly] public NativeList<T> OutList;

        public void Execute()
        {
            var count = Queue.Count;

            for (var i = 0; i < count; ++i)
                OutList.Add(Queue.Dequeue());
        }
    }
}