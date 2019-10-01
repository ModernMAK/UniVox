using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ECS.UniVox.VoxelChunk.Systems.ChunkJobs
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

    [BurstCompile]
    public struct DisposeArrayJob<T> : IJob where T : struct
    {
        public DisposeArrayJob(NativeArray<T> array)
        {
            Array = array;
        }

        [DeallocateOnJobCompletion] public NativeArray<T> Array;

        public void Execute()
        {
            //Do NOTHING
        }
    }
}