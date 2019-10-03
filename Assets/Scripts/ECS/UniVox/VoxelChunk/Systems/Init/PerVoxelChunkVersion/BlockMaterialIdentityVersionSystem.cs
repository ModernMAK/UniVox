using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class BlockMaterialIdentityVersionSystem : ChunkComponentDirtySystem<VoxelBlockMaterialIdentity,
        VoxelBlockMaterialIdentity.VersionProxyDirty>
    {
        protected override VoxelBlockMaterialIdentity.VersionProxyDirty GetInitialVersion()
        {
            return new VoxelBlockMaterialIdentity.VersionProxyDirty
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }
}