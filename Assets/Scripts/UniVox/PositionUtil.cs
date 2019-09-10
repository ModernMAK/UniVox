using System;
using Unity.Mathematics;

namespace Univox
{
    public static class PositionUtil
    {
        public static int3 GetLocalPosition(int3 worldPosition)
        {
            //TODO test that this gives expected results
            return worldPosition % Chunk.AxisSize;
        }

        public static int3 GetChunkPosition(int3 worldPosition)
        {
            //So truncating works on positive numbers but not negative ones.
            //We get around this by subtracting AxisSize-1; E.G AxisSize = 8, Subtract 7 (8-1)
            //THERFORE (15,-12,53) w/ AxisSize of 16; WE GET (0, -1, 3)
            //Without doing this, with integer truncation, we get (0, 0, 3)
            //We could get around this by using floats and flooring
            const int truncationShift = Chunk.AxisSize - 1;
            return (worldPosition - truncationShift) / Chunk.AxisSize;
        }

        public static void GetChunkAndLocalPosition(int3 worldPosition, out int3 chunkPosition,
            out int3 localPosition)
        {
            const int truncationShift = Chunk.AxisSize - 1;
            chunkPosition = (worldPosition - truncationShift) / Chunk.AxisSize;
            localPosition = worldPosition % Chunk.AxisSize;
        }

        public static int3 GetWorldPosition(int3 chunkPosition, int3 offset = default)
        {
            return chunkPosition * VoxelChunk.AxisSize + offset;
        }

        public static int3 GetLocalPosition(int index, AxisOrdering order = AxisOrdering.XYZ)
        {
            //Order is represented as
            //High - Mid - Low
            //If using the default, X = High, Y = Mid, Z = Low
            var low = index % VoxelChunk.AxisSize;
            var mid = (index / VoxelChunk.AxisSize) % VoxelChunk.AxisSize;
            var high = index / VoxelChunk.SquareSize;
            switch (order)
            {
                case AxisOrdering.XYZ:
                    return new int3(high, mid, low);
                case AxisOrdering.XZY:
                    return new int3(high, low, mid);
                case AxisOrdering.YXZ:
                    return new int3(mid, high, low);
                case AxisOrdering.YZX:
                    return new int3(low, high, mid);
                case AxisOrdering.ZXY:
                    return new int3(mid, low, high);
                case AxisOrdering.ZYX:
                    return new int3(low, mid, high);
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, null);
            }
        }

        public static int GetIndex(int3 localPosition, AxisOrdering order = AxisOrdering.XYZ)
        {
            return GetIndex(localPosition.x, localPosition.y, localPosition.z, order);
        }

        public static int GetIndex(int x, int y, int z, AxisOrdering order = AxisOrdering.XYZ)
        {
            int low, mid, high;
            switch (order)
            {
                case AxisOrdering.XYZ:
                    high = x;
                    mid = y;
                    low = z;
                    break;
                case AxisOrdering.XZY:
                    high = x;
                    mid = z;
                    low = y;
                    break;
                case AxisOrdering.YXZ:
                    high = y;
                    mid = x;
                    low = z;
                    break;
                case AxisOrdering.YZX:
                    high = y;
                    mid = z;
                    low = x;
                    break;
                case AxisOrdering.ZXY:
                    high = z;
                    mid = x;
                    low = y;
                    break;
                case AxisOrdering.ZYX:
                    high = z;
                    mid = y;
                    low = x;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, null);
            }

            return high * VoxelChunk.SquareSize + mid * VoxelChunk.AxisSize + low;
        }
    }
}