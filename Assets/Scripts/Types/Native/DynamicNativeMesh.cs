using Unity.Collections;
using UnityEngine;

namespace Types.Native
{
    public struct DynamicNativeMesh : INativeMesh
    {
        public DynamicNativeMesh(Allocator allocator) : this(ushort.MaxValue, allocator)
        {
        }

        public DynamicNativeMesh(int tris, Allocator allocator) : this(ushort.MaxValue, tris, allocator)
        {
        }

        public DynamicNativeMesh(int vertexes, int tris, Allocator allocator)
        {
            Vertexes = new NativeArray<Vector3>(vertexes, allocator);
            Normals = new NativeArray<Vector3>(vertexes, allocator);
            Tangents = new NativeArray<Vector4>(vertexes, allocator);
            Uv0 = new NativeArray<Vector2>(vertexes, allocator);
            Triangles = new NativeArray<int>(tris, allocator);
            VertexCount = new NativeValue<int>(allocator);
            TriangleCount = new NativeValue<int>(allocator);
        }

        public NativeArray<Vector3> Vertexes;
        public NativeArray<Vector3> Normals;
        public NativeArray<Vector4> Tangents;
        public NativeArray<Vector2> Uv0;
        public NativeArray<int> Triangles;
        public NativeValue<int> VertexCount;
        public NativeValue<int> TriangleCount;

        public void FillInto(Mesh m)
        {
            m.SetVertices(Vertexes, 0, VertexCount);
            m.SetNormals(Normals, 0, VertexCount);
            m.SetTangents(Tangents, 0, VertexCount);
            m.SetIndices(Triangles, 0, TriangleCount, MeshTopology.Triangles, 0);
            m.SetUVs(0, Uv0, 0, VertexCount);
        }

        public void Dispose()
        {
            Vertexes.Dispose();
            Normals.Dispose();
            Tangents.Dispose();
            Uv0.Dispose();
            Triangles.Dispose();
            VertexCount.Dispose();
            TriangleCount.Dispose();
        }
    }
}