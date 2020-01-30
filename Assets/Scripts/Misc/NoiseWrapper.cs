using Unity.Mathematics;

public static class NoiseWrapper
{
    
    /// <summary>
    /// Im sure theres a good reason its called cnoise
    /// But for readability, I made a wrapper
    /// 
    /// Also I understand it makes sense for Perlin to return -1,1
    /// But i could also make an arugment for everything to follow the same range [0,1]
    /// </summary>
    public static class Perlin
    {
        public static float Sample(float4 position) => noise.cnoise(position);
        public static float Sample(float3 position) => noise.cnoise(position);
        public static float Sample(float2 position) => noise.cnoise(position);

        public static float NormalizedSample(float4 position) => Normalize(Sample(position));
        public static float NormalizedSample(float3 position) => Normalize(Sample(position));
        public static float NormalizedSample(float2 position) => Normalize(Sample(position));


        public static float Normalize(float sample) => (sample + 1f) / 2f;
    }
}