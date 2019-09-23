using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Jobs
{
    public static class CommonRenderingJobs
    {
        /// <summary>
        /// Creates A Mesh. The Mesh is sent to teh GPU and is no longer readable.
        /// </summary>
        /// <param name="vertexes"></param>
        /// <param name="normals"></param>
        /// <param name="tangents"></param>
        /// <param name="uvs"></param>
        /// <param name="indexes"></param>
        /// <returns></returns>
        public static Mesh CreateMesh(NativeArray<float3> vertexes, NativeArray<float3> normals,
            NativeArray<float4> tangents, NativeArray<float2> uvs, NativeArray<int> indexes)
        {
            var mesh = new Mesh();
            mesh.SetVertices(vertexes);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetUVs(0, uvs);
            mesh.SetIndices(indexes, MeshTopology.Triangles, 0, false);
            //Optimizes the Mesh, might not be neccessary
            mesh.Optimize();
            //Recalculates the Mesh's Boundary
            mesh.RecalculateBounds();
            //Frees the mesh from CPU, but makes it unreadable.
//            mesh.UploadMeshData(true);
            return mesh;
        }
    }
}