using Unity.Mathematics;
using UnityEngine;
using UniVox.Types;
using UniVox.Types.Exceptions;
using UniVox.Utility;

namespace UniVox
{
    public static class UnivoxUtil
    {


        //An alias to help refactoring
        [System.Obsolete("No Longer Valid; Chunk size's are dynamic.")]
        private const int AxisSize = UnivoxDefine.AxisSize;
        [System.Obsolete("No Longer Valid; Chunk size's are dynamic.")]
        private const int CubeSize = UnivoxDefine.CubeSize;

        [System.Obsolete("No Longer Valid; Chunk size's are dynamic.")]
        public static bool IsPositionValid(int3 blockPosition)
        {
            var min = blockPosition >= 0;
            var max = blockPosition < AxisSize;
            return math.all(min) && math.all(max);
        }
        public static bool IsPositionValid(int3 blockPosition, int3 chunkSize)
        {
            var min = blockPosition >= 0;
            var max = blockPosition < chunkSize;
            return math.all(min) && math.all(max);
        }

        [System.Obsolete("No Longer Valid; Chunk size's are dynamic.")]
        public static bool IsIndexValid(int index)
        {
            return index >= 0 && index < CubeSize;
        }

        #region Index Conversion Util

        [System.Obsolete("No Longer Valid; Chunk size's are dynamic.")]
        public static int2 CreateSizeSquare()
        {
            return new int2(AxisSize);
        }

        [System.Obsolete("No Longer Valid; Chunk size's are dynamic.")]
        public static int3 CreateSizeCube()
        {
            throw new ObsoleteException();
            return new int3(AxisSize);
        }

        [System.Obsolete("No Longer Valid; Chunk size's are dynamic.")]
        public static int GetNeighborIndex(int3 position, Direction direction)
        {
            throw new ObsoleteException();
            return IndexMapUtil.ToIndex(position + direction.ToInt3(), CreateSizeCube());
        }

        public static int GetNeighborIndex(int3 position, int3 chunkSize, Direction direction)
        {
            return IndexMapUtil.ToIndex(position + direction.ToInt3(), chunkSize);
        }

        [System.Obsolete("No Longer Valid; Chunk size's are dynamic.")]
        public static int GetNeighborIndex(int index, Direction direction)
        {
            throw new ObsoleteException();
            return GetNeighborIndex(GetPosition3(index), direction);
        }

        [System.Obsolete("No Longer Valid; Chunk size's are dynamic.")]
        public static int GetIndex(int3 position)
        {
            throw new ObsoleteException();
            return IndexMapUtil.ToIndex(position, CreateSizeCube());
        }
        public static int GetIndex(int3 position, int3 chunkSize)
        {
            return IndexMapUtil.ToIndex(position, chunkSize);
        }

        public static int GetIndex(int x, int y, int z)
        {
            throw new ObsoleteException();
            return IndexMapUtil.ToIndex(x, y, z, AxisSize, AxisSize);
        }

        public static int GetIndex(int2 position)
        {
            throw new ObsoleteException();
            return IndexMapUtil.ToIndex(position, AxisSize);
        }

        public static int GetIndex(int x, int y)
        {
            throw new ObsoleteException();
            return IndexMapUtil.ToIndex(x, y, AxisSize);
        }

        #endregion

        #region Position Conversion Util

        [System.Obsolete("No Longer Valid; Chunk size's are dynamic.")]
        public static int3 GetPosition3(int index)
        {
            throw new ObsoleteException();
            return IndexMapUtil.ToPosition3(index, AxisSize, AxisSize);
        }

        public static int2 GetPosition2(int index)
        {
            throw new ObsoleteException();
            return IndexMapUtil.ToPosition2(index, AxisSize);
        }
        public static int3 GetPosition3(int index, int3 chunkSize)
        {
            return IndexMapUtil.ToPosition3(index, chunkSize.x, chunkSize.y);
        }


        #endregion

        #region System Conversion Util

        public static readonly float3 VoxelSpaceOffset = new float3(1f / 2f);
        private const float ReallySmallMultiplier = 1f / 100000f;

        [System.Obsolete("No Longer Valid; Chunk size's are dynamic.")]
        public static int3 ToWorldPosition(int3 chunkPosition, int3 blockPosition)
        {
            throw new ObsoleteException();
            return chunkPosition * AxisSize + blockPosition;
        }
        public static int3 ToWorldPosition(int3 chunkPosition, int3 blockPosition, int3 chunkSize)
        {
            return math.mul(chunkPosition, chunkSize) + blockPosition;
        }

        private static int3 ToValue(bool3 value, bool invert = false)
        {
            var x = value.x != invert ? 1 : 0;
            var y = value.y != invert ? 1 : 0;
            var z = value.z != invert ? 1 : 0;
            return new int3(x, y, z);
        }

        [System.Obsolete("No Longer Valid; Chunk size's are dynamic.")]
        public static int3 ToChunkPosition(int3 worldPosition)
        {
            throw new ObsoleteException();
            //

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
        public static int3 ToChunkPosition(int3 worldPosition, int3 chunkSize)
        {
            var needFix = worldPosition < 0;
            var value = ToValue(needFix);
            var negativeShift = value * (chunkSize - new int3(1,1,1));
            var chunkPosition = (worldPosition - negativeShift) / chunkSize;
            return chunkPosition;
        }

        [System.Obsolete("No Longer Valid; Chunk size's are dynamic.")]
        public static int3 ToBlockPosition(int3 worldPosition)
        {
            throw new ObsoleteException();
            var blockPosition = worldPosition % AxisSize;
            //Apply Shift - range from  1 to 2*AxisSize-1
            blockPosition += AxisSize;
            //Apply clamp - range from 0 to AxisSize-1
            blockPosition = blockPosition % AxisSize;
            return blockPosition;
        }
        public static int3 ToBlockPosition(int3 worldPosition, int3 chunkSize)
        {
            var blockPosition = worldPosition % chunkSize;
            //Apply Shift - range from  1 to 2*AxisSize-1
            blockPosition += chunkSize;
            //Apply clamp - range from 0 to AxisSize-1
            blockPosition = blockPosition % chunkSize;
            return blockPosition;
        }

        [System.Obsolete("No Longer Valid; Chunk size's are dynamic.")]
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

        public static Vector3 ToUnitySpace(float3 voxelPosition)
        {
            return voxelPosition + VoxelSpaceOffset;
        }

        #endregion
    }
}