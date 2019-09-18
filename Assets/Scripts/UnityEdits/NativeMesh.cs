using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEdits
{
    public class NativeMesh : IDisposable
    {
        public NativeMesh(Mesh mesh, Allocator allocator)
        {
            Layout = new LayoutInspector(mesh.GetVertexAttributes());

            var triCount = mesh.triangles.Length;
            Triangles = CreateAndFillArray(true, triCount, allocator, () => GetIndicesFromMesh(mesh));

            var vertCount = mesh.vertexCount;

            VertexCount = vertCount;
            IndexCount = triCount;

            Vertexes = CreateAndFillArray(Layout.HasPosition, vertCount, allocator,
                () => GetVertsFromMesh(mesh));
            Normals = CreateAndFillArray(Layout.HasNormal, vertCount, allocator,
                () => GetNormalsFromMesh(mesh));
            Tangents = CreateAndFillArray(Layout.HasTangent, vertCount, allocator,
                () => GetTangentsFromMesh(mesh));
            Uv0 = CreateAndFillArray(Layout.HasTexCoord0, vertCount, allocator, () => GetUvsFromMesh(mesh));
        }


        public LayoutInspector Layout { get; }

        public NativeArray<int> Triangles { get; }
        public NativeArray<float3> Vertexes { get; }
        public NativeArray<float3> Normals { get; }
        public NativeArray<float4> Tangents { get; }
        public NativeArray<float4> Uv0 { get; }

        public int VertexCount { get; }
        public int IndexCount { get; }

        public void Dispose()
        {
            Triangles.Dispose();
            Vertexes.Dispose();
            Normals.Dispose();
            Tangents.Dispose();
            Uv0.Dispose();
        }


        private static NativeArray<T> CreateAndFillArray<T>(bool isLegal, int size, Allocator allocator,
            Func<NativeArray<T>> getFunc) where T : struct
        {
            var arr = CreateArray<T>(isLegal, size, allocator);
            FillArray(isLegal, arr, getFunc);
            return arr;
        }

        private static NativeArray<T> CreateArray<T>(bool shouldInit, int size, Allocator allocator) where T : struct
        {
            return shouldInit
                ? new NativeArray<T>(size, allocator, NativeArrayOptions.UninitializedMemory)
                : new NativeArray<T>(0, Allocator.None);
        }

        private static void FillArray<T>(bool shouldFill, NativeArray<T> array, Func<NativeArray<T>> getFunc)
            where T : struct
        {
            if (shouldFill)
            {
                var source = getFunc();
                array.CopyFrom(source);
                source.Dispose();
            }
        }

        private static NativeArray<float3> GetVertsFromMesh(Mesh mesh)
        {
            var verts = mesh.vertices;
            var temp = new NativeArray<float3>(verts.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            for (var i = 0; i < verts.Length; i++) temp[i] = verts[i];

            return temp;
        }

        private static NativeArray<float3> GetNormalsFromMesh(Mesh mesh)
        {
            var verts = mesh.normals;
            var temp = new NativeArray<float3>(verts.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            for (var i = 0; i < verts.Length; i++) temp[i] = verts[i];

            return temp;
        }

        private static NativeArray<float4> GetTangentsFromMesh(Mesh mesh)
        {
            var verts = mesh.tangents;
            var temp = new NativeArray<float4>(verts.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            for (var i = 0; i < verts.Length; i++) temp[i] = verts[i];

            return temp;
        }

        private static NativeArray<float4> GetUvsFromMesh(Mesh mesh, int channel = 0)
        {
            var uvs = new List<Vector4>();
            mesh.GetUVs(channel, uvs);
            var temp = new NativeArray<float4>(uvs.Count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            for (var i = 0; i < uvs.Count; i++) temp[i] = uvs[i];

            return temp;
        }

        private NativeArray<int> GetIndicesFromMesh(Mesh mesh)
        {
            var indexes = mesh.triangles;
            var temp = new NativeArray<int>(indexes.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            for (var i = 0; i < indexes.Length; i++) temp[i] = indexes[i];

            return temp;
        }

        public struct LayoutInspector
        {
            public LayoutInspector(VertexAttributeDescriptor[] descriptors) : this()
            {
                foreach (var descriptor in descriptors)
                    switch (descriptor.attribute)
                    {
                        case VertexAttribute.Position:
                            HasPosition = true;
                            break;
                        case VertexAttribute.Normal:
                            HasNormal = true;
                            break;
                        case VertexAttribute.Tangent:
                            HasTangent = true;
                            break;
                        case VertexAttribute.Color:
                            HasColor = true;
                            break;
                        case VertexAttribute.TexCoord0:
                            HasTexCoord0 = true;
                            break;
                        case VertexAttribute.TexCoord1:
                            break;
                        case VertexAttribute.TexCoord2:
                            break;
                        case VertexAttribute.TexCoord3:
                            break;
                        case VertexAttribute.TexCoord4:
                            break;
                        case VertexAttribute.TexCoord5:
                            break;
                        case VertexAttribute.TexCoord6:
                            break;
                        case VertexAttribute.TexCoord7:
                            break;
                        case VertexAttribute.BlendWeight:
                            break;
                        case VertexAttribute.BlendIndices:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
            }

            public bool HasPosition { get; }
            public bool HasNormal { get; }
            public bool HasTangent { get; }
            public bool HasColor { get; }
            public bool HasTexCoord0 { get; }
        }
    }
}