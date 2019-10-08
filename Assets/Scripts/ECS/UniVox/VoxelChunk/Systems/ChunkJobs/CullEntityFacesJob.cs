using ECS.UniVox.VoxelChunk.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UniVox;
using UniVox.Types;

namespace ECS.UniVox.Systems
{
    [BurstCompile]
    public struct CullEntityFacesJob : IJob
    {
        [ReadOnly] public Entity Entity;

//            [ReadOnly] public ArchetypeChunkEntityType EntityType;


        public BufferFromEntity<VoxelData> GetVoxelBuffer;
        public NativeArray<VoxelRenderData> RenderData;


        public void Execute()
        {
            var directions = DirectionsX.GetDirectionsNative(Allocator.Temp);

            var voxelBuffer = GetVoxelBuffer[Entity];

            for (var blockIndex = 0; blockIndex < UnivoxDefine.CubeSize; blockIndex++)
            {
                var blockPos = UnivoxUtil.GetPosition3(blockIndex);
                var voxel = voxelBuffer[blockIndex];
                var render = RenderData[blockIndex];

                var primaryActive = voxel.Active;

                var hidden = DirectionsX.AllFlag;

                for (var dirIndex = 0; dirIndex < directions.Length; dirIndex++)
                {
                    var direction = directions[dirIndex];
                    var neighborPos = blockPos + direction.ToInt3();
                    var neighborIndex = UnivoxUtil.GetIndex(neighborPos);
                    var neighborActive = false;

                    if (UnivoxUtil.IsPositionValid(neighborPos))
                    {
                        var neighborVoxel = voxelBuffer[neighborIndex];
                        neighborActive = neighborVoxel.Active;
                    }

                    if (primaryActive && !neighborActive) hidden &= ~direction.ToFlag();
                }

                render = render.SetCullingFlags(hidden);
                RenderData[blockIndex] = render;
            }

            directions.Dispose();
        }
    }
}