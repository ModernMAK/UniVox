using System.Collections;
using System.Collections.Generic;
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

    private static JobHandle CalculateSNoise(int seed, float frequency, float resolution, NativeArray<int3> positions,
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
//
//        var noiseBJob = CalculateSNoiseNormalized(seed, frequency[1], resolution, positions, out var noiseB, noiseAJob);
//
//        var noiseCJob = CalculateSNoiseNormalized(seed, frequency[2], resolution, positions, out var noiseC, noiseBJob);
//        var noiseDJob = CalculateSNoiseNormalized(seed, frequency[2], resolution, positions, out var noiseD, noiseCJob);
//
        var deallocatePositions = new DeallocateNativeArrayJob<float3>(positions).Schedule(noiseJob);
//
//        var mergedNoise = new NativeArray<float>(size, Allocator.TempJob);
//        var mergedJob = new MergeOctaves4Job()
//        {
//            Merged = mergedNoise,
//            OctaveA = noiseA,
//            OctaveB = noiseB,
//            OctaveC = noiseC,
//            OctaveD = noiseD
//        }.SetDefaultScales().Schedule(size, 64, deallocatePositions);
//        
//        var deallocateNoiseA = new DeallocateNativeArrayJob<float>(noiseA).Schedule(mergedJob);
//        var deallocateNoiseB = new DeallocateNativeArrayJob<float>(noiseB).Schedule(deallocateNoiseA);
//        var deallocateNoiseC = new DeallocateNativeArrayJob<float>(noiseC).Schedule(deallocateNoiseB);
//        var deallocateNoiseD = new DeallocateNativeArrayJob<float>(noiseD).Schedule(deallocateNoiseC);
//        
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

        var a = RenderPartA(chunk, out var v, out var t, handle);
        a.Complete();
        var b = RenderPartB(chunk, v, t, out var nativeMesh, a);
        b.Complete();
        var c = RenderPartC(nativeMesh, mesh);
        c.Complete();
    }

    private static JobHandle RenderPartA(Chunk chunk, out NativeValue<int> vert, out NativeValue<int> tri,
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

    private static JobHandle RenderPartB(Chunk chunk, NativeValue<int> verts, NativeValue<int> tris,
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

    private static JobHandle RenderPartC(NativeMesh nativeMesh, Mesh mesh, JobHandle handle = default)
    {
        nativeMesh.FillInto(mesh);
        nativeMesh.Dispose();
        return handle;
    }

    public static IEnumerator RenderAsync(Chunk chunk, Mesh mesh, JobHandle handle = default)
    {
        //TODO - It will probably last more then a couple frames, but for now use tempjob instead of Persistant


        var partA = RenderPartA(chunk, out var v, out var t, handle);
        while (!partA.IsCompleted)
            yield return null;
        partA.Complete();

        var partB = RenderPartB(chunk, v, t, out var nativeMesh, partA);

        while (!partB.IsCompleted)
            yield return null;
        partB.Complete();
        var partC = RenderPartC(nativeMesh, mesh, partB);
        partC.Complete();
    }
}