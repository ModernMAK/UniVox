using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    public class VoxelDataVersionSystem : ChunkComponentDirtySystem<VoxelData, VoxelDataVersion>
    {
        protected override VoxelDataVersion GetInitialVersion()
        {
            return new VoxelDataVersion(ChangeVersionUtility.InitialGlobalSystemVersion);
        }
    }
}