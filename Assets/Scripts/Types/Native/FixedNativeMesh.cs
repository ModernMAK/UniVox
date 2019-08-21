using Unity.Collections;
using UnityEngine;

namespace Types.Native
{
    public struct FixedNativeMesh : INativeMesh
    {
        public FixedNativeMesh(int vertexes, int tris, Allocator allocator)
        {
            Vertexes = new NativeArray<Vector3>(vertexes, allocator);
            Normals = new NativeArray<Vector3>(vertexes, allocator);
            Tangents = new NativeArray<Vector4>(vertexes, allocator);
            Uv0 = new NativeArray<Vector2>(vertexes, allocator);
            Triangles = new NativeArray<int>(tris, allocator);
        }

        public NativeArray<Vector3> Vertexes;
        public NativeArray<Vector3> Normals;
        public NativeArray<Vector4> Tangents;
        public NativeArray<Vector2> Uv0;
        public NativeArray<int> Triangles;

        public void FillInto(Mesh m)
        {
            m.SetVertices(Vertexes);
            m.SetNormals(Normals);
            m.SetTangents(Tangents);
            m.SetTriangles(Triangles.ToArray(), 0);
            m.SetUVs(0, Uv0);
        }

        public void Dispose()
        {
            Vertexes.Dispose();
            Normals.Dispose();
            Tangents.Dispose();
            Uv0.Dispose();
            Triangles.Dispose();
        }
    }
}