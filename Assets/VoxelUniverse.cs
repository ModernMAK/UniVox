using System;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using UniVox.Utility;

public class VoxelUniverse : IDisposable
{
    public readonly Dictionary<byte, VoxelWorld> WorldMap;

    public VoxelUniverse()
    {
        WorldMap = new Dictionary<byte, VoxelWorld>();
    }

    public void Dispose()
    {
        foreach (var value in WorldMap.Values)
        {
            value.Dispose();
        }
    }
}


public abstract class AbstractGenerator<TKey, TValue>
{
    public abstract void Generate(TKey key, TValue value);
    public abstract JobHandle Generate(TKey key, TValue value, JobHandle depends);
}

public class VoxelChunkGenerator : AbstractGenerator<int3, VoxelChunk>
{
    public int Seed;
    public float Solidity;

    private float SamplePerlin(float3 position)
    {
        var sample = noise.cnoise(new float4(position.x, position.y, position.z, Seed));
        //Remap sample to [0f,1f]
        sample += 1f;
        sample /= 2f;
        return sample;
    }

    private float SamplePerlin(float3 position, params float2[] octaves)
    {
        var runningSample = 0f;
        var runningScale = 0f;
        for (var i = 0; i < octaves.Length; i++)
        {
            var octave = octaves[i];
            var sample = SamplePerlin(position * octave.x);
            var scaledSample = sample * octave.y;

            runningSample += scaledSample;
            runningScale += octave.y;
        }

        if (runningScale > 0f)
            return runningSample / runningScale;
        return 0f;
    }

    private float GetSolidSample(float3 position)
    {
        return SamplePerlin(position,
            new float2(1f / 8f, 1f),
            new float2(1f / 16f, 4f),
            new float2(1f / 32f, 16f));
    }

    private float GetIdentitySample(float3 position)
    {
        return SamplePerlin(position,
            new float2(1f / 2f, 4f),
            new float2(1f / 4f, 2f),
            new float2(1f / 8f, 1f));
    }

    public override void Generate(int3 chunkWorldPosition, VoxelChunk chunk)
    {
        var indexConverter = new IndexConverter3D(chunk.ChunkSize);
        var active = chunk.Active;
        var ids = chunk.Identities;
        for (var i = 0; i < chunk.Active.Length; i++)
        {
            var positionOffset = indexConverter.Expand(i);
            var worldPosition = chunkWorldPosition + positionOffset;


            var solidSample = GetSolidSample(worldPosition);
            var idSample = GetIdentitySample(worldPosition);
            active[i] = (solidSample <= Solidity);
            ids[i] = (byte) (int) math.lerp(byte.MinValue, byte.MaxValue, idSample);
        }
    }

    public override JobHandle Generate(int3 key, VoxelChunk value, JobHandle depends)
    {
        throw new NotImplementedException();
    }
}