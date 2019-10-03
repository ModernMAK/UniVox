using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ECS.UniVox.VoxelChunk.Systems.ChunkJobs
{
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