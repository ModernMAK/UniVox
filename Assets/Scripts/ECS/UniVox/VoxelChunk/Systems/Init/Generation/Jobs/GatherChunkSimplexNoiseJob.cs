using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UniVox;

namespace ECS.UniVox.Systems.Jobs
{
    /// <summary>
    ///     Samples the chunk with the given seed, provides some sampler controls in the form of frequency, amplitude, shift
    ///     (an offset for the input), and bias (an offset for the output)
    /// </summary>
    [BurstCompile]
    public struct GatherChunkSimplexNoiseJob : IJobParallelFor
    {
        [ReadOnly] public int3 ChunkPosition;
        [ReadOnly] public NoiseSampler Sampler;

        [NativeMatchesParallelForLength] [WriteOnly]
        public NativeArray<float> Values;


        public void Execute(int index)
        {
            var blockPos = UnivoxUtil.GetPosition3(index);
            var worldPos = blockPos + ChunkPosition * UnivoxDefine.AxisSize;

            var samplePos = worldPos * Sampler.Frequency + Sampler.Shift;
            var seededSamplePos = new float4(samplePos.x, samplePos.y, samplePos.z, Sampler.Seed);


            var sampleValue = noise.snoise(seededSamplePos);
            var scaledSampleValue = sampleValue * Sampler.Amplitude;
            var biasedSampleValue = scaledSampleValue + Sampler.Bias;


            Values[index] = biasedSampleValue;
        }

        public const int JobSize = UnivoxDefine.CubeSize;
    }
}