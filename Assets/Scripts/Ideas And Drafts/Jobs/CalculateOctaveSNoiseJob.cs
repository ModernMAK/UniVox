using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Jobs
{
    [BurstCompile]
    public struct CalculateOctaveSNoiseJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<float> Noise;
        [ReadOnly] public NativeArray<float3> Positions;
        [ReadOnly] public int Seed;

        [NativeDisableParallelForRestriction] [ReadOnly]
        public NativeArray<float3> OctaveOffset;

        [NativeDisableParallelForRestriction] [ReadOnly]
        public NativeArray<float3> Frequency;

        [NativeDisableParallelForRestriction] [ReadOnly]
        public NativeArray<float> Amplitude;

        [ReadOnly] public float TotalAmplitude;
        [ReadOnly] public int Octaves;

        public void Execute(int index)
        {
            var position = Positions[index];
            var mergedSample = 0f;

            for (var octave = 0; octave < Octaves; octave++)
            {
                var octavePos = (position + OctaveOffset[octave]) * Frequency[octave];
                var samplerPosition = new float4(octavePos.x, octavePos.y, octavePos.z, Seed);
                mergedSample += Amplitude[octave] * math.unlerp(-1, 1, noise.snoise(samplerPosition));
            }


            Noise[index] = mergedSample / TotalAmplitude;
        }
    }
}