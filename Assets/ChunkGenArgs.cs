using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class ChunkGenArgs
{
    [Serializable]
    public struct Data
    {
        public float Scale;
        public float Resolution;
        public float3 Frequency => Resolution.Equals(0f) ? 0f : (Scale / Resolution);
        public float Amplitude;
        public float3 Offset;
    }

    public int seed;
    [Range(0f, 1f)] public float threshold;
    public Data[] array;

    public NativeChunkGenArgs ToNative(Allocator allocator)
    {
        var size = array.Length;
        var native = new NativeChunkGenArgs()
        {
            Frequency = new NativeArray<float3>(size, allocator),
            Amplitude = new NativeArray<float>(size, allocator),
            Offset = new NativeArray<float3>(size, allocator),
            Seed = seed,
            Threshold = threshold,
            Length = size
        };
        for (var i = 0; i < size; i++)
        {
            native.Frequency[i] = array[i].Frequency;
            native.Amplitude[i] = array[i].Amplitude;
            native.Offset[i] = array[i].Offset;
            native.TotalAmplitude += array[i].Amplitude;
        }

        return native;
    }
}