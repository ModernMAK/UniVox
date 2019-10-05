using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class BlockMaterialIdentityVersionSystem : ChunkComponentDirtySystem<VoxelBlockMaterialIdentity,
        VoxelBlockMaterialIdentity.Version>
    {
        protected override VoxelBlockMaterialIdentity.Version GetInitialVersion()
        {
            return new VoxelBlockMaterialIdentity.Version
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }
}