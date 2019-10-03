using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class BlockSubMaterialIdentityVersionSystem : ChunkComponentDirtySystem<VoxelBlockSubMaterial,
        VoxelBlockSubMaterial.VersionProxyDirty>
    {
        protected override VoxelBlockSubMaterial.VersionProxyDirty GetInitialVersion()
        {
            return new VoxelBlockSubMaterial.VersionProxyDirty
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }
}