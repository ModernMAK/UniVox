using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jobs;
using Rendering;
using Types;
using Types.Native;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class RenderUtilV2
{
    public static JobHandle VisiblityPass(Chunk chunk, JobHandle handle = default)
    {
        var job = new UpdateHiddenFacesJob
        {
            Directions = DirectionsX.GetDirectionsNative(Allocator.TempJob),
            HiddenFaces = chunk.HiddenFaces,
            Active = chunk.ActiveFlags
        }.Schedule(Chunk.FlatSize, 64, handle);
        return job;
    }

    private static JobHandle CalculateSNoise(int seed, float frequency, float resolution, NativeArray<float3> positions,
        out NativeArray<float> noise, JobHandle handle = default)
    {
        var size = positions.Length;
        var sampler = new NativeArray<float4>(size, Allocator.TempJob);
        var samplerJob = new CalculateNoiseSampler4DJob()
        {
            Seed = seed,
            Scale = frequency / resolution,
            Positions = positions,
            Sampler = sampler
        }.Schedule(size, 64, handle);

        noise = new NativeArray<float>(size, Allocator.TempJob);
        var noiseJob = new CalculateSNoiseFromSamplerJob()
        {
            Noise = noise,
            Sampler = sampler
        }.Schedule(size, 64, samplerJob);

        var deallocateSampler = new DeallocateNativeArrayJob<float4>(sampler).Schedule(noiseJob);

        return deallocateSampler;
    }

    private static JobHandle CalculateSNoiseNormalized(int seed, float frequency, float resolution,
        NativeArray<float3> positions, out NativeArray<float> noise, JobHandle handle = default)
    {
        var calculateJob = CalculateSNoise(seed, frequency, resolution, positions, out noise, handle);
        var normalizeJob = new CalculateNormalizedNoiseJob()
        {
            Noise = noise,
            NoiseMin = -1f,
            NosieMax = 1f
        }.Schedule(noise.Length, 64, calculateJob);
        return normalizeJob;
    }

    public static JobHandle GenerationPass(int seed, int3 chunkPos, Chunk chunk, float frequency,
        float resolution,
        float threshold = 0.5f,
        JobHandle handle = default)
    {
        const int size = Chunk.FlatSize;
        const int scale = Chunk.AxisSize;
        var positions = new NativeArray<float3>(size, Allocator.TempJob);
        var positionJob = new GatherWorldPositions()
        {
            ChunkOffset = chunkPos * scale,
            Positions = positions
        }.Schedule(size, 64, handle);

        var noiseJob =
            CalculateSNoiseNormalized(seed, frequency, resolution, positions, out var noise, positionJob);

        var deallocatePositions = new DeallocateNativeArrayJob<float3>(positions).Schedule(noiseJob);

        var activeJob = new CalculateActiveFromNoise()
        {
            Active = chunk.ActiveFlags,
            Threshold = threshold,
            Noise = noise
        }.Schedule(size, 64, deallocatePositions);


        var deallocateNoise = new DeallocateNativeArrayJob<float>(noise).Schedule(activeJob);

        return deallocateNoise;
    }

    public static void Render(Chunk chunk, Mesh mesh, JobHandle handle = default)
    {
        //TODO - It will probably last more then a couple frames, but for now use tempjob instead of Persistant

        var a = CalculateMeshSizePass(chunk, out var v, out var t, handle);
        a.Complete();
        var b = GenerateMeshPass(chunk, v, t, out var nativeMesh, a);
        b.Complete();
        var c = UpdateMeshPass(nativeMesh, mesh);
        c.Complete();
    }

    private static JobHandle CalculateMeshSizePass(Chunk chunk, out NativeValue<int> vert, out NativeValue<int> tri,
        JobHandle handle = default)
    {
        vert = new NativeValue<int>(Allocator.TempJob);
        tri = new NativeValue<int>(Allocator.TempJob);
        var v = new NativeArray<int>(Chunk.FlatSize, Allocator.TempJob);
        var t = new NativeArray<int>(Chunk.FlatSize, Allocator.TempJob);
        var gatherSizeJob = new CalculateMeshSizePerBlockJob
        {
            Directions = DirectionsX.GetDirectionsNative(Allocator.TempJob),
            HiddenFaces = chunk.HiddenFaces,
            Rotations = chunk.Rotations,
            Shapes = chunk.Shapes,
            Triangles = t,
            Vertexes = v
        }.Schedule(Chunk.FlatSize, 64, handle);

        var flattenVertsJob =
            new SumAndDiscardNativeArray {Values = v, Result = vert}.Schedule(gatherSizeJob);
        var flattenTrisJob =
            new SumAndDiscardNativeArray {Values = t, Result = tri}.Schedule(flattenVertsJob);

        return flattenTrisJob;
    }

    private static JobHandle GenerateMeshPass(Chunk chunk, NativeValue<int> verts, NativeValue<int> tris,
        out NativeMesh nativeMesh,
        JobHandle handle = default)
    {
        nativeMesh = new NativeMesh(verts, tris, Allocator.TempJob);
        verts.Dispose();
        tris.Dispose();
        var generateJob = new GenerateBoxelMeshV2
        {
            Rotations = chunk.Rotations,
            NativeCube = new NativeCubeBuilder(Allocator.TempJob),
            Directions = DirectionsX.GetDirectionsNative(Allocator.TempJob),
            HiddenFaces = chunk.HiddenFaces,
            NativeMesh = nativeMesh,
            Shapes = chunk.Shapes,
            VertexPos = 0,
            TrianglePos = 0,
            WorldOffset = new float3(1f / 2f)
        }.Schedule(handle);
        return generateJob;
    }

    private static JobHandle UpdateMeshPass(NativeMesh nativeMesh, Mesh mesh, JobHandle handle = default)
    {
        nativeMesh.FillInto(mesh);
        nativeMesh.Dispose();
        return handle;
    }

    public static IEnumerator RenderCoroutine(Chunk chunk, Mesh mesh, JobHandle handle = default)
    {
        //TODO - It will probably last more then a couple frames, but for now use tempjob instead of Persistant


        var partA = CalculateMeshSizePass(chunk, out var v, out var t, handle);
        while (!partA.IsCompleted)
            yield return null;
        partA.Complete();

        var partB = GenerateMeshPass(chunk, v, t, out var nativeMesh, partA);

        while (!partB.IsCompleted)
            yield return null;
        partB.Complete();
        var partC = UpdateMeshPass(nativeMesh, mesh, partB);
        partC.Complete();
    }

    public static async Task<JobHandle> RenderAsync(Chunk chunk, Mesh mesh, JobHandle handle = default,
        int millisecondStep = 100)
    {
        //TODO - It will probably last more then a couple frames, but for now use tempjob instead of Persistant


        var partA = CalculateMeshSizePass(chunk, out var v, out var t, handle);

        while (!partA.IsCompleted)
            await Task.Delay(millisecondStep);

        partA.Complete();

        var partB = GenerateMeshPass(chunk, v, t, out var nativeMesh, partA);

        while (!partB.IsCompleted)
            await Task.Delay(millisecondStep);

        partB.Complete();

        var partC = UpdateMeshPass(nativeMesh, mesh, partB);

        while (!partC.IsCompleted)
            await Task.Delay(millisecondStep);
        return partC;
    }

    public static JobHandle GenerationOctavePass(int3 chunkPos, Chunk chunk, ChunkGenArgs args,
        JobHandle handle = default)
    {
        const int size = Chunk.FlatSize;
        const int scale = Chunk.AxisSize;
        var positions = new NativeArray<float3>(size, Allocator.TempJob);
        var positionJob = new GatherWorldPositions()
        {
            ChunkOffset = chunkPos * scale,
            Positions = positions
        }.Schedule(size, 64, handle);

        var noise = new NativeArray<float>(size, Allocator.TempJob);

        var nativeArgs = args.ToNative(Allocator.TempJob);

        var noiseJob = new CalculateOctaveSNoiseJob()
        {
            Amplitude = nativeArgs.Amplitude,
            Frequency = nativeArgs.Frequency,
            Noise = noise,
            OctaveOffset = nativeArgs.Offset,
            Octaves = nativeArgs.Length,
            Positions = positions,
            Seed = nativeArgs.Seed,
            TotalAmplitude = nativeArgs.TotalAmplitude
        }.Schedule(size, 64, positionJob);

        var deallocateAmplitude = new DeallocateNativeArrayJob<float>(nativeArgs.Amplitude).Schedule(noiseJob);
        var deallocateFrequency =
            new DeallocateNativeArrayJob<float>(nativeArgs.Frequency).Schedule(deallocateAmplitude);
        var deallocateOffset = new DeallocateNativeArrayJob<float3>(nativeArgs.Offset).Schedule(deallocateFrequency);

        var deallocatePositions = new DeallocateNativeArrayJob<float3>(positions).Schedule(deallocateOffset);

        var activeJob = new CalculateActiveFromNoise()
        {
            Active = chunk.ActiveFlags,
            Threshold = nativeArgs.Threshold,
            Noise = noise
        }.Schedule(size, 64, deallocatePositions);


        var deallocateNoise = new DeallocateNativeArrayJob<float>(noise).Schedule(activeJob);

        return deallocateNoise;
    }
}