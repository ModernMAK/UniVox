using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UniVox.Types.Native;

namespace UniVox.Rendering.ChunkGen.Jobs
{
    [BurstCompile]
    public struct CalculateIndexAndTotalSizeJob : IJob
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> VertexSizes;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> TriangleSizes;

        [WriteOnly] public NativeArray<int> VertexOffsets;
        [WriteOnly] public NativeValue<int> VertexTotalSize;

        [WriteOnly] public NativeArray<int> TriangleOffsets;
        [WriteOnly] public NativeValue<int> TriangleTotalSize;

        public void Execute()
        {
            var vertexTotal = 0;
            var triangleTotal = 0;
            for (var i = 0; i < VertexSizes.Length; i++)
            {
                VertexOffsets[i] = vertexTotal;
                vertexTotal += VertexSizes[i];


                TriangleOffsets[i] = triangleTotal;
                triangleTotal += TriangleSizes[i];
            }

            VertexTotalSize.Value = vertexTotal;
            TriangleTotalSize.Value = triangleTotal;
        }
    }
}