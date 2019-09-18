using Types;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UniVox;

namespace UnityEdits
{
    [BurstCompile]
    internal struct CreateTransformsForChunk : IJobParallelFor
    {
        [ReadOnly] public float3 PositionOffset;
        [ReadOnly] public int3 ChunkSize;
        [ReadOnly] public AxisOrdering Ordering;
        [WriteOnly] public NativeArray<float4x4> Transforms;


        private int3 IndexToOrganized(int index)
        {
            return AxisOrderingX.Reorder(PositionToIndexUtil.ToPosition3(index, ChunkSize), Ordering);
        }

        private static float3x3 CreateRotation()
        {
            return new float3x3
            {
                c0 = new float3(1, 0, 0),
                c1 = new float3(0, 1, 0),
                c2 = new float3(0, 0, 1)
            };
        }

        public void Execute(int chunkIndex)
        {
            var positionFromIndex = IndexToOrganized(chunkIndex);
            var rotation = CreateRotation();

            Transforms[chunkIndex] = new float4x4(rotation, positionFromIndex + PositionOffset);
        }
    }

    [BurstCompile]
    internal struct CreatePositionsForChunk : IJobParallelFor
    {
        [ReadOnly] public float3 PositionOffset;
        [ReadOnly] public int3 ChunkSize;
        [ReadOnly] public AxisOrdering Ordering;
        [WriteOnly] public NativeArray<float3> Positions;


        private int3 IndexToOrganized(int index)
        {
            return AxisOrderingX.Reorder(PositionToIndexUtil.ToPosition3(index, ChunkSize), Ordering);
        }

        public void Execute(int chunkIndex)
        {
            Positions[chunkIndex] = IndexToOrganized(chunkIndex);
        }
    }
}