using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace UnityEdits.Rendering
{
    [BurstCompile]
    struct GatherVoxelRenderData : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
        [ReadOnly] public ArchetypeChunkSharedComponentType<VoxelRenderData> VoxelRenderDataType;
        [WriteOnly] public NativeArray<int> ChunkRenderer;

        public void Execute(int chunkIndex)
        {
            var chunk = Chunks[chunkIndex];
            var sharedIndex = chunk.GetSharedComponentIndex(VoxelRenderDataType);
            ChunkRenderer[chunkIndex] = sharedIndex;
        }
    }
}