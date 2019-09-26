using Types;
using Unity.Mathematics;
using UnityEdits;
using UnityEngine;

namespace UniVox
{
    public static class UnivoxUtil
    {
        //An alias to help refactoring
        private const int AxisSize = UnivoxDefine.AxisSize;

        public static bool IsValid(int3 blockPosition)
        {
            var min = blockPosition >= 0;
            var max = blockPosition < AxisSize;
            return math.all(min) && math.all(max);
        }

        #region Index Conversion Util

    
        public static int2 CreateSizeSquare() => new int2(AxisSize);
        public static int3 CreateSizeCube() => new int3(AxisSize);
        public static int GetIndex(int3 position) => IndexMapUtil.ToIndex(position, CreateSizeCube());
        public static int GetIndex(int x, int y, int z) => IndexMapUtil.ToIndex(x, y, z, AxisSize, AxisSize);
        public static int GetIndex(int2 position) => IndexMapUtil.ToIndex(position, AxisSize);
        public static int GetIndex(int x, int y) => IndexMapUtil.ToIndex(x, y, AxisSize);

        #endregion
        #region Position Conversion Util
        public static int3 GetPosition3(int index) => IndexMapUtil.ToPosition3(index, AxisSize, AxisSize);
        public static int2 GetPosition2(int index) => IndexMapUtil.ToPosition2(index, AxisSize);

        #endregion
    
        #region Position Conversion Util
        private static readonly float3 VoxelSpaceOffset = new float3(1f / 2f);
        private const float ReallySmallMultiplier = 1f / 100f;
    
        public static int3 ToWorldPosition(int3 chunkPosition, int3 blockPosition)
        {
            return chunkPosition * AxisSize + blockPosition;
        }
        public static int3 ToChunkPosition(int3 worldPosition)
        {
            return worldPosition / AxisSize;
        }
        public static int3 ToBlockPosition(int3 worldPosition)
        {
            return worldPosition % AxisSize;
        }

        public static void SplitPosition(int3 worldPosition, out int3 chunkPosition, out int3 blockPosition)
        {
            chunkPosition = worldPosition / AxisSize;
            blockPosition = worldPosition % AxisSize;
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