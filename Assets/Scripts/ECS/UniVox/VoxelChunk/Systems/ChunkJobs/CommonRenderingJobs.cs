using ECS.UniVox.VoxelChunk.Systems;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace ECS.UniVox.Systems
{
    public static class CommonRenderingJobs
    {
        /// <summary>
        ///     Creates A Value. The Value is sent to teh GPU and is no longer readable.
        /// </summary>
        /// <param name="vertexes"></param>
        /// <param name="normals"></param>
        /// <param name="tangents"></param>
        /// <param name="uvs"></param>
        /// <param name="indexes"></param>
        /// <returns></returns>
        public static Mesh CreateMesh(NativeArray<float3> vertexes, NativeArray<float3> normals,
            NativeArray<float4> tangents, NativeArray<float3> uvs, NativeArray<int> indexes)
        {
            var mesh = new Mesh();
            mesh.SetVertices(vertexes);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetUVs(0, uvs);
            //            mesh.SetUVs(2, uv1s);
            mesh.SetIndices(indexes, MeshTopology.Triangles, 0, false);
            //Optimizes the Value, might not be neccessary
            mesh.Optimize();
            //Recalculates the Value's Boundary
            mesh.RecalculateBounds();
            //Frees the mesh from CPU, but makes it unreadable.
            //            mesh.UploadMeshData(true);
            return mesh;
        }

        public static Mesh CreateMesh(NativeArray<float3> vertexes, NativeArray<float3> normals,
            NativeArray<float4> tangents, NativeArray<float3> uvs, NativeArray<int> indexes, int vStart, int vLen,
            int iStart, int iLen)
        {
            var mesh = new Mesh();
            mesh.SetVertices(vertexes, vStart, vLen);
            mesh.SetNormals(normals, vStart, vLen);
            mesh.SetTangents(tangents, vStart, vLen);
            mesh.SetUVs(0, uvs, vStart, vLen);
            //            mesh.SetUVs(2, uv1s);
            mesh.SetIndices(indexes, iStart, iLen, MeshTopology.Triangles, 0, false);
            //Optimizes the Value, might not be neccessary
            mesh.Optimize();
            //Recalculates the Value's Boundary
            mesh.RecalculateBounds();
            //Frees the mesh from CPU, but makes it unreadable.
            //            mesh.UploadMeshData(true);
            return mesh;
        }

        public static Mesh CreateMesh(NativeArray<VertexBufferComponent> vertexes,
            NativeArray<NormalBufferComponent> normals,
            NativeArray<TangentBufferComponent> tangents, NativeArray<TextureMap0BufferComponent> uvs,
            NativeArray<IndexBufferComponent> indexes)
        {
            var mesh = new Mesh();
            mesh.SetVertices(vertexes);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetUVs(0, uvs);
//            mesh.SetUVs(2, uv1s);

            mesh.SetIndices(indexes, MeshTopology.Triangles, 0, false);

            //Optimizes the Value, might not be neccessary
            mesh.Optimize();
            //Recalculates the Value's Boundary
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}