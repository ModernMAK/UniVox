using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class BlockMaterialIdentityVersionSystem : ChunkComponentDirtySystem<BlockMaterialIdentityComponent,
        BlockMaterialIdentityComponent.Version>
    {
        protected override BlockMaterialIdentityComponent.Version GetInitialVersion()
        {
            return new BlockMaterialIdentityComponent.Version()
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }
}