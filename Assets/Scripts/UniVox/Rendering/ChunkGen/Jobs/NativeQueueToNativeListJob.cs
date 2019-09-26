using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UniVox.Rendering.ChunkGen.Jobs
{
    [BurstCompile]
    public struct NativeQueueToNativeListJob<T> : IJob where T : struct
    {
        public NativeQueue<T> queue;
        [WriteOnly] public NativeList<T> out_list;

        public void Execute()
        {
            var count = queue.Count;

            for (var i = 0; i < count; ++i)
                out_list.Add(queue.Dequeue());
        }
    }
}