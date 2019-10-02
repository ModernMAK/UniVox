using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class BlockSubMaterialIdentityVersionSystem : ChunkComponentDirtySystem<BlockSubMaterialIdentityComponent,
        BlockSubMaterialIdentityComponent.Version>
    {
        protected override BlockSubMaterialIdentityComponent.Version GetInitialVersion()
        {
            return new BlockSubMaterialIdentityComponent.Version()
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }
}