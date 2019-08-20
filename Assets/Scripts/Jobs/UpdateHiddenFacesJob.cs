using Types;
using Types.Native;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Jobs
{
    [BurstCompile]
    public struct UpdateHiddenFacesJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<bool> Solid;
        [WriteOnly] public NativeArray<Directions> HiddenFaces;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Direction> Directions;

        //If facing a solid, the block is hidden

        private static bool IsValid(int3 pos)
        {
            return pos.x <= VoxelPos8.MaxValue && pos.x >= VoxelPos8.MinValue && pos.y <= VoxelPos8.MaxValue &&
                   pos.y >= VoxelPos8.MinValue && pos.z <= VoxelPos8.MaxValue && pos.z >= VoxelPos8.MinValue;
        }

        public void Execute(int index)
        {
            var oPos = new VoxelPos8(index).Position;
            var flags = DirectionsX.NoneFlag;
            for (var i = 0; i < 6; i++)
            {
                var dir = Directions[i];
                var dPos = oPos + dir.ToInt3();
                if (!IsValid(dPos))
                    continue;

                var vPos = new VoxelPos8(dPos);
                if (Solid[vPos])
                    flags |= dir.ToFlag();
            }

            HiddenFaces[index] = flags;
        }
    }
}