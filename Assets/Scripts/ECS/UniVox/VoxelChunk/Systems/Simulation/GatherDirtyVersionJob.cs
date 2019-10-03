using ECS.UniVox.VoxelChunk.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [BurstCompile]
    public struct GatherDirtyVersionJob<TVersion> : IJob
        where TVersion : struct, IVersionProxy<TVersion>, IComponentData
    {
        [ReadOnly] public ArchetypeChunk Chunk;

        public ArchetypeChunkComponentType<TVersion> VersionsType;

        [ReadOnly] public NativeArray<TVersion> CurrentVersions;

        [WriteOnly] public NativeArray<bool> Ignore;


        public void Execute()
        {
            var entityVersions = Chunk.GetNativeArray(VersionsType);
            for (var index = 0; index < Chunk.Count; index++)
            {
                var entityVersion = entityVersions[index];
                var currentVersion = CurrentVersions[index];

                if (currentVersion.DidChange(entityVersion))
                {
                    entityVersions[index] = currentVersion;
                    Ignore[index] = false;
                }
                else
                {
                    Ignore[index] = true;
                }
            }
        }
    }
}