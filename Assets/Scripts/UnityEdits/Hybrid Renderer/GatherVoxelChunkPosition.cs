using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace UnityEdits.Rendering
{
    [BurstCompile]
    struct GatherVoxelChunkPosition : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
        [ReadOnly] public ArchetypeChunkSharedComponentType<ChunkPosition> VoxelRenderDataType;
        [WriteOnly] public NativeArray<int> ChunkPositions;

        public void Execute(int chunkIndex)
        {
            var chunk = Chunks[chunkIndex];
            var sharedIndex = chunk.GetSharedComponentIndex(VoxelRenderDataType);
            ChunkPositions[chunkIndex] = sharedIndex;
        }
    }
}