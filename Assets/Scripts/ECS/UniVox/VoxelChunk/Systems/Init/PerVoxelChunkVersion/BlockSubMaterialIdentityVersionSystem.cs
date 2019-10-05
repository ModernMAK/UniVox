using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class BlockSubMaterialIdentityVersionSystem : ChunkComponentDirtySystem<VoxelBlockSubMaterial,
        VoxelBlockSubMaterial.Version>
    {
        protected override VoxelBlockSubMaterial.Version GetInitialVersion()
        {
            return new VoxelBlockSubMaterial.Version
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }
}