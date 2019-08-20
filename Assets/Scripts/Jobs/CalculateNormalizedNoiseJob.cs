using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Jobs
{
    [BurstCompile]
    public struct CalculateNormalizedNoiseJob : IJobParallelFor
    {
        public NativeArray<float> Noise;
        [ReadOnly] public float NoiseMin;
        [ReadOnly] public float NosieMax;

        public void Execute(int index)
        {
            Noise[index] = math.unlerp(NoiseMin, NosieMax, Noise[index]);
        }
    }

    [BurstCompile]
    public struct MergeOctaves2Job : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> OctaveA;
        [ReadOnly] public float ScaleA;
        [ReadOnly] public NativeArray<float> OctaveB;
        [ReadOnly] public float ScaleB;
        [WriteOnly] public NativeArray<float> Merged;

        /// <summary>
        /// Sets the default scale values for the job (A=2/3, B = 1/3 etc)
        /// </summary>
        /// <returns>The job with default scales.</returns>
        public MergeOctaves2Job SetDefaultScales()
        {
            const float Total = 5f;
            ScaleA = 4f / Total;
            ScaleB = 1f / Total;
            
            return this;
        }
        
        public void Execute(int index)
        {
            Merged[index] = ScaleA * OctaveA[index] + ScaleB * OctaveB[index];
        }
    }
    [BurstCompile]
    public struct MergeOctaves3Job : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> OctaveA;
        [ReadOnly] public float ScaleA;
        [ReadOnly] public NativeArray<float> OctaveB;
        [ReadOnly] public float ScaleB;
        [ReadOnly] public NativeArray<float> OctaveC;
        [ReadOnly] public float ScaleC;
        [WriteOnly] public NativeArray<float> Merged;

        /// <summary>
        /// Sets the default scale values for the job (A=4/7, B = 2/7 etc)
        /// </summary>
        /// <returns>The job with default scales.</returns>
        public MergeOctaves3Job SetDefaultScales()
        {
            const float Total = 21f;
            ScaleA = 16f / Total;
            ScaleB = 4f / Total;
            ScaleC = 1f / Total;
            
            return this;
        }
        
        public void Execute(int index)
        {
            Merged[index] = ScaleA * OctaveA[index]
                            + ScaleB * OctaveB[index]
                            + ScaleC * OctaveC[index];
        }
    }
    [BurstCompile]
    public struct MergeOctaves4Job : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> OctaveA;
        [ReadOnly] public float ScaleA;
        [ReadOnly] public NativeArray<float> OctaveB;
        [ReadOnly] public float ScaleB;
        [ReadOnly] public NativeArray<float> OctaveC;
        [ReadOnly] public float ScaleC;
        [ReadOnly] public NativeArray<float> OctaveD;
        [ReadOnly] public float ScaleD;
        [WriteOnly] public NativeArray<float> Merged;

        /// <summary>
        /// Sets the default scale values for the job (A=8/15, B = 4/15 etc)
        /// </summary>
        /// <returns>The job with default scales.</returns>
        public MergeOctaves4Job SetDefaultScales()
        {
            const float Total = 64f+16f+4f+1f;
            ScaleB = 64f / Total;
            ScaleB = 16f / Total;
            ScaleC = 4f / Total;
            ScaleD = 1f / Total;
            return this;
        }

        public void Execute(int index)
        {
            Merged[index] = ScaleA * OctaveA[index]
                            + ScaleB * OctaveB[index]
                            + ScaleC * OctaveC[index]
                            + ScaleD * OctaveD[index];
        }
    }
}