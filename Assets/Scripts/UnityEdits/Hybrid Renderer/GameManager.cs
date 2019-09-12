using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace UnityEdits.Rendering
{
    public static class GameManager
    {
        public static readonly MasterRegistry MasterRegistry = new MasterRegistry();

        public static NativeMesh GetNativeMesh(this Mesh mesh, Allocator allocator)
        {
            return new NativeMesh(mesh, allocator);
        }

        public static void CreateMergeMeshJob(NativeMesh nativeMesh, NativeArray<float4x4> matrixes,
            Mesh result)
        {
            var meshBuilder = new NativeMeshBuilder(nativeMesh.Layout, nativeMesh.VertexCount * matrixes.Length,
                nativeMesh.IndexCount * matrixes.Length, Allocator.TempJob);
            var job = new MergeMeshJob()
            {
                Layout = nativeMesh.Layout,
                Matrixes = matrixes,

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
            job.Schedule(matrixes.Length, BatchSize).Complete();

            meshBuilder.LoadIntoMesh(result, true);
        }
    }

    public struct MergeMeshJob : IJobParallelFor
    {
        [ReadOnly] public NativeMesh.LayoutInspector Layout;

        [NativeMatchesParallelForLength] [ReadOnly]
        public NativeArray<float4x4> Matrixes;


        [ReadOnly] public NativeArray<float3> MeshVertex;
        [ReadOnly] public NativeArray<float3> MeshNormal;
        [ReadOnly] public NativeArray<float4> MeshTangent;
        [ReadOnly] public NativeArray<float4> MeshUv;
        [ReadOnly] public NativeArray<int> MeshTriangles;

        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeArray<float3> MergedVertex;

        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeArray<float3> MergedNormal;

        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeArray<float4> MergedTangent;

        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeArray<float4> MergedUv;

        [NativeDisableParallelForRestriction] [WriteOnly]
        public NativeArray<int> MergedTriangles;


        [ReadOnly] public int MeshVertexCount;
        [ReadOnly] public int MeshTriangleCount;

        public void CopyTriangles(int index)
        {
            for (var j = 0; j < MeshTriangleCount; j++)
                MergedTriangles[index * MeshTriangleCount + j] = MeshTriangles[j] + index * MeshVertexCount;
        }


        public void CopyVertex(int index)
        {
            for (var j = 0; j < MeshVertexCount; j++)
                MergedVertex[index * MeshVertexCount + j] = math.transform(Matrixes[index], MeshVertex[j]);
        }


        public void CopyNormal(int index)
        {
            for (var j = 0; j < MeshVertexCount; j++)
                MergedNormal[index * MeshVertexCount + j] = math.transform(Matrixes[index], MeshNormal[j]);
        }


        public void CopyTangent(int index)
        {
            for (var j = 0; j < MeshVertexCount; j++)
            {
                var tangent = MeshTangent[j];
                var transformedTangent = math.transform(Matrixes[index], tangent.xyz);

                var transformedTangentHanded = new float4(transformedTangent.x, transformedTangent.y,
                    transformedTangent.z, tangent.w);

                MergedTangent[index * MeshVertexCount + j] = transformedTangentHanded;
            }
        }


        public void CopyUv0(int index)
        {
            for (var j = 0; j < MeshVertexCount; j++)
            {
                MergedUv[index * MeshVertexCount + j] = MeshUv[j];
            }
        }


        public void Execute(int index)
        {
            if (Layout.HasPosition)
                CopyVertex(index);
            if (Layout.HasNormal)
                CopyNormal(index);
            if (Layout.HasTangent)
                CopyTangent(index);
            if (Layout.HasTexCoord0)
                CopyUv0(index);
            
            CopyTriangles(index);
        }
    }

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

    public class NativeMesh : IDisposable
    {
        public struct LayoutInspector
        {
            public LayoutInspector(VertexAttributeDescriptor[] descriptors) : this()
            {
                foreach (var descriptor in descriptors)
                {
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
            }

            public bool HasPosition { get; private set; }
            public bool HasNormal { get; private set; }
            public bool HasTangent { get; private set; }
            public bool HasColor { get; private set; }
            public bool HasTexCoord0 { get; private set; }
        }


        public NativeMesh(Mesh mesh, Allocator allocator)
        {
            Layout = new LayoutInspector(mesh.GetVertexAttributes());

            var triCount = mesh.triangles.Length;
            Triangles = CreateAndFillArray<int>(true,triCount, allocator,()=>GetIndicesFromMesh(mesh));

            var vertCount = mesh.vertexCount;

            VertexCount = vertCount;
            IndexCount = triCount;

            Vertexes = CreateAndFillArray<float3>(Layout.HasPosition, vertCount, allocator,
                () => GetVertsFromMesh(mesh));
            Normals = CreateAndFillArray<float3>(Layout.HasNormal, vertCount, allocator,
                () => GetNormalsFromMesh(mesh));
            Tangents = CreateAndFillArray<float4>(Layout.HasTangent, vertCount, allocator,
                () => GetTangentsFromMesh(mesh));
            Uv0 = CreateAndFillArray<float4>(Layout.HasTexCoord0, vertCount, allocator, () => GetUvsFromMesh(mesh, 0));
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

            for (var i = 0; i < verts.Length; i++)
            {
                temp[i] = verts[i];
            }

            return temp;
        }

        private static NativeArray<float3> GetNormalsFromMesh(Mesh mesh)
        {
            var verts = mesh.normals;
            var temp = new NativeArray<float3>(verts.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            for (var i = 0; i < verts.Length; i++)
            {
                temp[i] = verts[i];
            }

            return temp;
        }

        private static NativeArray<float4> GetTangentsFromMesh(Mesh mesh)
        {
            var verts = mesh.tangents;
            var temp = new NativeArray<float4>(verts.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            for (var i = 0; i < verts.Length; i++)
            {
                temp[i] = verts[i];
            }

            return temp;
        }

        private static NativeArray<float4> GetUvsFromMesh(Mesh mesh, int channel = 0)
        {
            var uvs = new List<Vector4>();
            mesh.GetUVs(channel, uvs);
            var temp = new NativeArray<float4>(uvs.Count, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            for (var i = 0; i < uvs.Count; i++)
            {
                temp[i] = uvs[i];
            }

            return temp;
        }
        
        private NativeArray<int> GetIndicesFromMesh(Mesh mesh)
        {

            var indexes = mesh.triangles;
            var temp = new NativeArray<int>(indexes.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            for (var i = 0; i < indexes.Length; i++)
            {
                temp[i] = indexes[i];
            }

            return temp;
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
    }
}