using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class
        BlockShapeComponentVersionSystem : ChunkComponentDirtySystem<VoxelBlockShape, VoxelBlockShape.VersionProxyDirty>
    {
        protected override VoxelBlockShape.VersionProxyDirty GetInitialVersion()
        {
            return new VoxelBlockShape.VersionProxyDirty
            {
                Value = ChangeVersionUtility.InitialGlobalSystemVersion
            };
        }
    }
}