using Unity.Mathematics;
using UnityEngine;
using UniVox.Types;
using UniVox.Utility;

namespace UniVox
{
    public static class UnivoxUtil
    {
        //An alias to help refactoring
        private const int AxisSize = UnivoxDefine.AxisSize;
        private const int CubeSize = UnivoxDefine.CubeSize;

        public static bool IsPositionValid(int3 blockPosition)
        {
            var min = blockPosition >= 0;
            var max = blockPosition < AxisSize;
            return math.all(min) && math.all(max);
        }

        public static bool IsIndexValid(int index)
        {
            return index >= 0 && index < CubeSize;
        }

        #region Index Conversion Util

        public static int2 CreateSizeSquare()
        {
            return new int2(AxisSize);
        }

        public static int3 CreateSizeCube()
        {
            return new int3(AxisSize);
        }

        public static int GetNeighborIndex(int3 position, Direction direction)
        {
            return IndexMapUtil.ToIndex(position + direction.ToInt3(), CreateSizeCube());
        }

        public static int GetNeighborIndex(int index, Direction direction)
        {
            return GetNeighborIndex(GetPosition3(index), direction);
        }

        public static int GetIndex(int3 position)
        {
            return IndexMapUtil.ToIndex(position, CreateSizeCube());
        }

        public static int GetIndex(int x, int y, int z)
        {
            return IndexMapUtil.ToIndex(x, y, z, AxisSize, AxisSize);
        }

        public static int GetIndex(int2 position)
        {
            return IndexMapUtil.ToIndex(position, AxisSize);
        }

        public static int GetIndex(int x, int y)
        {
            return IndexMapUtil.ToIndex(x, y, AxisSize);
        }

        #endregion

        #region Position Conversion Util

        public static int3 GetPosition3(int index)
        {
            return IndexMapUtil.ToPosition3(index, AxisSize, AxisSize);
        }

        public static int2 GetPosition2(int index)
        {
            return IndexMapUtil.ToPosition2(index, AxisSize);
        }

        #endregion

        #region System Conversion Util

        private static readonly float3 VoxelSpaceOffset = new float3(1f / 2f);
        private const float ReallySmallMultiplier = 1f / 100f;

        public static int3 ToWorldPosition(int3 chunkPosition, int3 blockPosition)
        {
            return chunkPosition * AxisSize + blockPosition;
        }

        private static int3 ToValue(bool3 value, bool invert = false)
        {
            var x = value.x != invert ? 1 : 0;
            var y = value.y != invert ? 1 : 0;
            var z = value.z != invert ? 1 : 0;
            return new int3(x, y, z);
        }

        public static int3 ToChunkPosition(int3 worldPosition)
        {
            //WHAT WE WANT
            //0 to 31 => 0
            //-32 to -1 => -1

            //WHAT WE HAVE
            //-31 to 31 => 0
            //-63 to -32 => -1

            //HOW TO FIX?
            //If we add 31 IF the value is negatve
            //0 to 31 => 0
            var needFix = worldPosition < 0;
            var value = ToValue(needFix);
            var negativeShift = value * (AxisSize - 1);
            var chunkPosition = (worldPosition - negativeShift) / AxisSize;
            return chunkPosition;
        }

        public static int3 ToBlockPosition(int3 worldPosition)
        {
            var blockPosition = worldPosition % AxisSize;
            //Apply Shift - range from  1 to 2*AxisSize-1
            blockPosition += AxisSize;
            //Apply clamp - range from 0 to AxisSize-1
            blockPosition = blockPosition % AxisSize;
            return blockPosition;
        }

        public static void SplitPosition(int3 worldPosition, out int3 chunkPosition, out int3 blockPosition)
        {
            chunkPosition = ToChunkPosition(worldPosition);
            //Apply clamp - range from -AxisSize+1 to AxisSize-1
            blockPosition = ToBlockPosition(worldPosition);
        }

        #endregion

        #region Space Conversion

        public static int3 ToVoxelSpace(float3 position)
        {
            return ToVoxelSpace(position, float3.zero);
        }


        public static int3 ToVoxelSpace(float3 position, float3 normal)
        {
            normal *= ReallySmallMultiplier;
            var fixedPosition = position - normal;

            return new int3(math.floor(fixedPosition));
        }

        public static Vector3 ToUnitySpace(int3 voxelPosition)
        {
            return voxelPosition + VoxelSpaceOffset;
        }

        #endregion
    }
}