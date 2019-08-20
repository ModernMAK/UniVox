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
        [ReadOnly] public float Frequency;
        [ReadOnly] public float Amplitude;

        public void Execute(int index)
        {
            var tempPos = Positions[index] + OctaveOffset;
            tempPos *= Frequency;
            var samplerPosition = new float4(tempPos.x, tempPos.y, tempPos.z, Seed);
            var sample = noise.snoise(samplerPosition);
            sample = math.unlerp(-1, 1, sample);
            Noise[index] = Amplitude * sample;
        }
    }

    [BurstCompile]
    public struct CalculateOctaveSNoiseJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<float> Noise;
        [ReadOnly] public NativeArray<float3> Positions;
        [ReadOnly] public int Seed;

        [NativeDisableParallelForRestriction] [ReadOnly]
        public NativeArray<float3> OctaveOffset;

        [NativeDisableParallelForRestriction] [ReadOnly]
        public NativeArray<float> Frequency;

        [NativeDisableParallelForRestriction] [ReadOnly]
        public NativeArray<float> Amplitude;

        [ReadOnly] public float TotalAmplitude;
        [ReadOnly] public int Octaves;

        public void Execute(int index)
        {
            var pos = Positions[index];
            var mergedSample = 0f;
            for (var octave = 0; octave < Octaves; octave++)
            {
                var octavePos = (pos + OctaveOffset[octave]) * Frequency[octave];
                var samplerPosition = new float4(octavePos.x, octavePos.y, octavePos.z, Seed);
                var sample = noise.snoise(samplerPosition);
                sample = math.unlerp(-1, 1, sample);
                sample *= Amplitude[octave];
                mergedSample += sample;
            }

            mergedSample /= TotalAmplitude;

            Noise[index] = mergedSample;
        }
    }
}