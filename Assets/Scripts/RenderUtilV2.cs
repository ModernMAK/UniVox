using System;
using System.Collections;
using Jobs;
using Rendering;
using Types;
using Types.Native;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[NativeContainer]
public struct NativeValue<T> : IDisposable where T : struct
{
    public NativeValue(Allocator allocator)
    {
        _array = new NativeArray<T>(1, allocator);
    }
    
    private NativeArray<T> _array;

    public T Value
    {
        get => _array[0];
        set => _array[0] = value;
    }

    public static implicit operator T(NativeValue<T> nativeValue)
    {
        return nativeValue.Value;
    }
    
    public void Dispose()
    {
        _array.Dispose();
    }
}

public static class RenderUtilV2
{
    public static JobHandle VisiblityPass(Chunk chunk, JobHandle handle = default)
    {
        var job = new UpdateHiddenFacesJob
        {
            Directions = DirectionsX.GetDirectionsNative(Allocator.TempJob),
            HiddenFaces = chunk.HiddenFaces,
            Solid = chunk.SolidFlags
        }.Schedule(Chunk.FlatSize, 64, handle);
        return job;
    }

    public static void Render(Chunk chunk, Mesh mesh, JobHandle handle = default)
    {
        //TODO - It will probably last more then a couple frames, but for now use tempjob instead of Persistant

        var a = RenderPartA(chunk, out var v, out var t, handle);
        a.Complete();
        var b = RenderPartB(chunk, v, t, out var nativeMesh, a);
        b.Complete();
        RenderPartC(nativeMesh, mesh).Complete();
    }

    private static JobHandle RenderPartA(Chunk chunk, out NativeArray<int> vert, out NativeArray<int> tri,
        JobHandle handle = default)
    {
        vert = new NativeArray<int>(1, Allocator.TempJob);
        tri = new NativeArray<int>(1, Allocator.TempJob);
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

    private static JobHandle RenderPartB(Chunk chunk, NativeArray<int> verts, NativeArray<int> tris,
        out NativeMesh nativeMesh,
        JobHandle handle = default)
    {
        nativeMesh = new NativeMesh(verts[0], tris[0], Allocator.TempJob);
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
            WorldOffset = new float3(1f / 2f),
        }.Schedule(handle);
        return generateJob;
    }

    private static JobHandle RenderPartC(NativeMesh nativeMesh, Mesh mesh, JobHandle handle = default)
    {
        nativeMesh.FillInto(mesh);
        nativeMesh.Dispose();
        return default;
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
        RenderPartC(nativeMesh, mesh, partB);
    }
}