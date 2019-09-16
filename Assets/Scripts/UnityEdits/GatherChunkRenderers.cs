using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;

namespace UnityEdits.Rendering
{
    [BurstCompile]
    struct GatherChunkRenderers : IJobParallelFor
    {
        [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
        [ReadOnly] public ArchetypeChunkSharedComponentType<RenderMesh> RenderMeshType;
        public NativeArray<int> ChunkRenderer;

        public void Execute(int chunkIndex)
        {
            var chunk = Chunks[chunkIndex];
            var sharedIndex = chunk.GetSharedComponentIndex(RenderMeshType);
            ChunkRenderer[chunkIndex] = sharedIndex;
        }
    }
}