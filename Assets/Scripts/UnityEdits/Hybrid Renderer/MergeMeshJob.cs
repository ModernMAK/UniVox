using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEdits.Hybrid_Renderer
{
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

        [ReadOnly] public int MatrixCount;

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
            for (var j = 0; j < MeshVertexCount; j++) MergedUv[index * MeshVertexCount + j] = MeshUv[j];
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
}