using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class
        BlockIdentityVersionSystem : ChunkComponentDirtySystem<BlockIdentityComponent, BlockIdentityComponent.Version>
    {
        protected override BlockIdentityComponent.Version GetInitialVersion()
        {
            return new BlockIdentityComponent.Version()
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }
}