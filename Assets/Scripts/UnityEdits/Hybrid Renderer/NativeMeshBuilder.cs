using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace UnityEdits.Rendering
{
    public class NativeMeshBuilder : IDisposable
    {
        public NativeMeshBuilder(NativeMesh.LayoutInspector layout, int vertCount, int triCount, Allocator allocator)
        {
            Layout = layout;
            VertexCount = vertCount;
            IndexCount = triCount;

            Triangles = new NativeArray<int>(triCount, allocator, NativeArrayOptions.UninitializedMemory);


            Vertexes = CreateArray<float3>(Layout.HasPosition, vertCount, allocator);
            Normals = CreateArray<float3>(Layout.HasNormal, vertCount, allocator);
            Tangents = CreateArray<float4>(Layout.HasTangent, vertCount, allocator);
            Uv0 = CreateArray<float4>(Layout.HasTexCoord0, vertCount, allocator);
        }

        private static NativeArray<T> CreateArray<T>(bool shouldInit, int size, Allocator allocator) where T : struct
        {
            return shouldInit
                ? new NativeArray<T>(size, allocator, NativeArrayOptions.UninitializedMemory)
                : new NativeArray<T>(0, Allocator.None);
        }


        private static void LoadIntoMesh(NativeMeshBuilder builder, Mesh mesh, bool disposeAfter = false)
        {
            mesh.Clear(true);
            if (builder.Layout.HasPosition)
                mesh.SetVertices(builder.Vertexes);

            if (builder.Layout.HasNormal)
                mesh.SetNormals(builder.Normals);

            if (builder.Layout.HasTangent)
                mesh.SetTangents(builder.Tangents);

            if (builder.Layout.HasTexCoord0)
                mesh.SetUVs(0, builder.Uv0);

            mesh.SetIndices(builder.Triangles, MeshTopology.Triangles, 0);
            if (disposeAfter)
                builder.Dispose();
        }

        public void LoadIntoMesh(Mesh mesh, bool disposeAfter = false)
        {
            LoadIntoMesh(this, mesh, disposeAfter);
        }

        public NativeMesh.LayoutInspector Layout { get; }

        public int VertexCount { get; }
        public int IndexCount { get; }

        public NativeArray<int> Triangles { get; }
        public NativeArray<float3> Vertexes { get; }
        public NativeArray<float3> Normals { get; }
        public NativeArray<float4> Tangents { get; }
        public NativeArray<float4> Uv0 { get; }

        public void Dispose()
        {
            Triangles.Dispose();
            Vertexes.Dispose();
            Normals.Dispose();
            Tangents.Dispose();
            Uv0.Dispose();
        }
    }
}