using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Jobs
{
    [BurstCompile]
    public struct CalculateSNoiseFromSamplerJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<float> Noise;
        [ReadOnly] public NativeArray<float4> Sampler;

        public void Execute(int index)
        {
            Noise[index] = noise.snoise(Sampler[index]);
        }
    }
    [BurstCompile]
    public struct CalculateSNoiseJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<float> Noise;
        [ReadOnly] public NativeArray<float3> Positions;
        [ReadOnly] public int Seed;
        [ReadOnly] public float3 OctaveOffset;

        public void Execute(int index)
        {
            Noise[index] = noise.snoise(Sampler[index]);
        }
    }
}