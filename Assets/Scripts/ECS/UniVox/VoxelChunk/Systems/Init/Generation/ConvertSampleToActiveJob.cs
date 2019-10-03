using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ECS.UniVox.VoxelChunk.Systems.Generation
{
    [BurstCompile]
    public struct ConvertSampleToActiveJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> Sample;
        [WriteOnly] public NativeArray<bool> Active;

        [ReadOnly] public float Threshold;

        public void Execute(int index)
        {
            //Separate lines for easier debugging
            var sample = Sample[index];
            var active = sample > Threshold;
            Active[index] = active;
        }
    }
}