using ECS.UniVox.VoxelChunk.Components;
using ECS.UniVox.VoxelChunk.Systems;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class
        BlockActiveVersionSystem : ChunkComponentDirtySystem<BlockActiveComponent, BlockActiveComponent.Version>
    {
        protected override BlockActiveComponent.Version GetInitialVersion()
        {
            return new BlockActiveComponent.Version()
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }
}