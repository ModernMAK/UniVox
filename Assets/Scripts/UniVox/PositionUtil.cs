using Unity.Mathematics;
using UnityEdits;

namespace UniVox
{
    public static class PositionUtil
    {
        public static int3 GetLocalPosition(int3 worldPosition)
        {
            //TODO test that this gives expected results
            return worldPosition % ChunkSize.AxisSize;
        }

        public static int3 GetChunkPosition(int3 worldPosition)
        {
            //So truncating works on positive numbers but not negative ones.
            //We get around this by subtracting AxisSize-1; E.G AxisSize = 8, Subtract 7 (8-1)
            //THERFORE (15,-12,53) w/ AxisSize of 16; WE GET (0, -1, 3)
            //Without doing this, with integer truncation, we get (0, 0, 3)
            //We could get around this by using floats and flooring
            const int truncationShift = ChunkSize.AxisSize - 1;
            return (worldPosition - truncationShift) / ChunkSize.AxisSize;
        }

        public static void GetChunkAndLocalPosition(int3 worldPosition, out int3 chunkPosition,
            out int3 localPosition)
        {
            const int truncationShift = ChunkSize.AxisSize - 1;
            chunkPosition = (worldPosition - truncationShift) / ChunkSize.AxisSize;
            localPosition = worldPosition % ChunkSize.AxisSize;
        }

        public static int3 GetWorldPosition(int3 chunkPosition, int3 offset = default)
        {
            return chunkPosition * ChunkSize.AxisSize + offset;
        }
    }
}