using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Jobs
{
    [BurstCompile]
    public struct CalculateNoiseSampler4DJob : IJobParallelFor
    {
        [ReadOnly] public int Seed;
        [ReadOnly] public float Scale;
        [ReadOnly] public NativeArray<float3> Positions;
        [WriteOnly] public NativeArray<float4> Sampler;
        

        public void Execute(int index)
        {
            var pos = (float3) Positions[index];
            pos *= Scale;
            Sampler[index] = new float4(pos.x, pos.y, pos.z, Seed);
        }
    }
}