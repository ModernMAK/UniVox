using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Jobs
{
    [BurstCompile]
    public struct CalculateActiveFromNoise : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> Noise;
        [WriteOnly] public NativeArray<bool> Active;
        [ReadOnly] public float Threshold;

        public void Execute(int index)
        {
            Active[index] = (Noise[index] <= Threshold);
        }
    }
}