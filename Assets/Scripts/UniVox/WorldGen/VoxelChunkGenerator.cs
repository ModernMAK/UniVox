using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
    using UniVox.Utility;
    using UniVox.WorldGen;

public class VoxelChunkGenerator : AbstractGenerator<int3, VoxelChunk>
{
    public int Seed;
    public float Solidity;


    private struct CalculateActive : IJobParallelFor
    {
        public int Seed;
        public IndexConverter3D Converter;
        public float Solidity;
        public float3 Offset;
        public NativeArray<bool> Active;
        public float3 Scale;


        public void Execute(int index)
        {
            var pos = Converter.Expand(index) + Offset;
            pos *= Scale;
            var sample = NoiseWrapper.Perlin.NormalizedSample(new float4(pos, Seed));
            Active[index] = sample <= Solidity;
        }
    }

    private struct CalculateIdentity : IJobParallelFor
    {
        public int Seed;
        public IndexConverter3D Converter;
        public float3 Offset;
        public NativeArray<byte> Identities;
        public float3 Scale;


        public void Execute(int index)
        {
            var pos = Converter.Expand(index) + Offset;
            pos *= Scale;
            var sample = NoiseWrapper.Perlin.NormalizedSample(new float4(pos, Seed));
            Identities[index] = (byte) (int) math.lerp(byte.MinValue, byte.MaxValue, sample);
        }
    }

    public override JobHandle Generate(int3 key, VoxelChunk value, JobHandle depends)
    {
        var converter = new IndexConverter3D(value.ChunkSize);
        var invSize = 1f / (float3) value.ChunkSize;
        const int innerBatchSize = byte.MaxValue;
        depends = new CalculateActive()
        {
            Active = value.Active,
            Converter = converter,
            Offset = key,
            Scale = invSize,
            Seed = Seed,
            Solidity = Solidity
        }.Schedule(value.Active.Length, innerBatchSize, depends);

        depends = new CalculateIdentity()
        {
            Identities = value.Identities,
            Converter = converter,
            Offset = key,
            Scale = invSize,
            Seed = Seed,
        }.Schedule(value.Active.Length, innerBatchSize, depends);

        return depends;
    }
}