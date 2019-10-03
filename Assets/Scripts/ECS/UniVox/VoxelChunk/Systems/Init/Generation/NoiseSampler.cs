using System;
using Unity.Mathematics;

namespace Unity.Entities
{
    //Container to make passing this information easier
    [Serializable]
    public struct NoiseSampler
    {
        public float Seed;
        public float3 Frequency;
        public float3 Shift;
        public float Amplitude;
        public float Bias;
    }
}