using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Univox;

namespace UnityEdits.Rendering
{
    [BurstCompile]
    struct CreateTransformsForChunk : IJobParallelFor
    {
        [ReadOnly] public int3 ChunkPosition;
        [ReadOnly] public int3 ChunkSize;
        [ReadOnly] public AxisOrdering Ordering;
        [WriteOnly] public NativeArray<float4x4> Transformers;


        private int3 IndexToXyz(int index)
        {
            var xSize = ChunkSize.x;
            var ySize = ChunkSize.y;
            var zSize = ChunkSize.z;
            var xySize = ySize * xSize;

            var x = index % xSize;
            var y = (index / xSize) % ySize;
            var z = (index / xySize) % zSize;
            return new int3(x, y, z);
        }

        private int3 IndexToOrganized(int index) => AxisOrderingX.Reorder(IndexToXyz(index), Ordering);

        private static float3x3 CreateRotation()
        {
            return new float3x3()
            {
                c0 = new float3(1, 0, 0),
                c1 = new float3(0, 1, 0),
                c2 = new float3(0, 0, 1),
            };
        }

        public void Execute(int chunkIndex)
        {
            var positionFromIndex = IndexToOrganized(chunkIndex);
            var rotation = CreateRotation();

            Transformers[chunkIndex] = new float4x4(rotation, positionFromIndex + ChunkPosition);
        }
    }
    [BurstCompile]
    struct CreateRenderGroupChunk : IJobParallelFor
    {
        [ReadOnly] public int3 ChunkPosition;
        [ReadOnly] public int3 ChunkSize;
        [ReadOnly] public AxisOrdering Ordering;
        [WriteOnly] public NativeArray<float4x4> Transformers;


        private int3 IndexToXyz(int index)
        {
            var xSize = ChunkSize.x;
            var ySize = ChunkSize.y;
            var zSize = ChunkSize.z;
            var xySize = ySize * xSize;

            var x = index % xSize;
            var y = (index / xSize) % ySize;
            var z = (index / xySize) % zSize;
            return new int3(x, y, z);
        }

        private int3 IndexToOrganized(int index) => AxisOrderingX.Reorder(IndexToXyz(index), Ordering);

        private static float3x3 CreateRotation()
        {
            return new float3x3()
            {
                c0 = new float3(1, 0, 0),
                c1 = new float3(0, 1, 0),
                c2 = new float3(0, 0, 1),
            };
        }

        public void Execute(int chunkIndex)
        {
            var positionFromIndex = IndexToOrganized(chunkIndex);
            var rotation = CreateRotation();

            Transformers[chunkIndex] = new float4x4(rotation, positionFromIndex + ChunkPosition);
        }
    }
}