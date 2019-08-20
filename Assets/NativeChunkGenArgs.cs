using System;
using Unity.Collections;
using Unity.Mathematics;

public struct NativeChunkGenArgs : IDisposable
{
    public NativeArray<float> Frequency;
    public NativeArray<float> Amplitude;
    public NativeArray<float3> Offset;
    public float Threshold;
    public int Seed;
    public int Length;
    public float TotalAmplitude;

    public void Dispose()
    {
        Frequency.Dispose();
        Amplitude.Dispose();
        Offset.Dispose();
    }
}