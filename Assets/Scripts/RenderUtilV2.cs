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
            Solid = chunk.SolidTable
        }.Schedule(Chunk.FlatSize, 64);
        return job;
    }

    public static void Render(Chunk chunk, Mesh mesh, JobHandle handle = default)
    {
        //TODO - It will probably last more then a couple frames, but for now use tempjob instead of Persistant
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

        var results = new NativeArray<int>(2, Allocator.TempJob);
        var flattenVertsJob =
            new SumAndDiscardNativeArray {Values = v, Result = results, Index = 0}.Schedule(gatherSizeJob);
        var flattenTrisJob =
            new SumAndDiscardNativeArray {Values = t, Result = results, Index = 1}.Schedule(flattenVertsJob);
        flattenTrisJob.Complete();

        var native = new NativeMesh(results[0], results[1], Allocator.TempJob);
        results.Dispose();
        var generateJob = new GenerateBoxelMeshV2
        {
            Rotations = chunk.Rotations,
            NativeCube = new NativeCubeBuilder(Allocator.TempJob),
            Directions = DirectionsX.GetDirectionsNative(Allocator.TempJob),
            HiddenFaces = chunk.HiddenFaces,
            NativeMesh = native,
            Shapes = chunk.Shapes,
            VertexPos = 0,
            TrianglePos = 0,
            WorldOffset = new float3(1f / 2f)
        }.Schedule();
        generateJob.Complete();
        native.FillInto(mesh);
        native.Dispose();
    }
}