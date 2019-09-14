using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEdits.Rendering;
using UnityEngine;

static internal class NativeMeshUtil
{
    public static NativeMesh GetNativeMesh(this Mesh mesh, Allocator allocator)
    {
        return new NativeMesh(mesh, allocator);
    }

    public static void CreateMergeMeshJob(NativeMesh nativeMesh, NativeArray<float4x4> matrixes,
        Mesh result)
    {
        CreateMergeMeshJob(nativeMesh, matrixes, matrixes.Length, result);
    }

    public static void CreateMergeMeshJob(NativeMesh nativeMesh, NativeArray<float4x4> matrixes, int matrixCount,
        Mesh result)
    {
        var meshBuilder = new NativeMeshBuilder(nativeMesh.Layout, nativeMesh.VertexCount * matrixes.Length,
            nativeMesh.IndexCount * matrixes.Length, Allocator.TempJob);
        var job = new MergeMeshJob()
        {
            Layout = nativeMesh.Layout,
            Matrixes = matrixes,
            MatrixCount = matrixCount,

            MeshNormal = nativeMesh.Normals,
            MergedNormal = meshBuilder.Normals,


            MeshTangent = nativeMesh.Tangents,
            MergedTangent = meshBuilder.Tangents,


            MeshVertex = nativeMesh.Vertexes,
            MergedVertex = meshBuilder.Vertexes,

            MeshUv = nativeMesh.Uv0,
            MergedUv = meshBuilder.Uv0,

            MeshTriangles = nativeMesh.Triangles,
            MergedTriangles = meshBuilder.Triangles,

            MeshVertexCount = nativeMesh.VertexCount,
            MeshTriangleCount = nativeMesh.IndexCount
        };

        const int BatchSize = 1024;
        job.Schedule(matrixCount, BatchSize).Complete();

        meshBuilder.LoadIntoMesh(result, true);
    }
}