using Types;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Jobs
{
    [BurstCompile]
    public struct GatherWorldPositions : IJobParallelFor
    {
        [ReadOnly] public int3 ChunkOffset;
        [WriteOnly] public NativeArray<float3> Positions;

        public void Execute(int index)
        {
            Positions[index] = ChunkOffset + new VoxelPos8(index).Position;
        }
    }
}