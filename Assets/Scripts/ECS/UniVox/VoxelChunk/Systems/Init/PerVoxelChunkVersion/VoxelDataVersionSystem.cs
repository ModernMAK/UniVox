using System;
using ECS.UniVox.Systems.Generic;
using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class VoxelDataVersionSystem : ChunkComponentDirtySystem<VoxelData, VoxelDataVersion>
    {
        protected override VoxelDataVersion GetInitialVersion()
        {
            //TODO
            throw new InvalidOperationException("return new VoxelDataVersion(ChangeVersionUtility.InitialGlobalSystemVersion);");
//            return new VoxelDataVersion(ChangeVersionUtility.InitialGlobalSystemVersion);
        }
    }
}