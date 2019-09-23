using Types;
using Unity.Mathematics;
using UnityEngine;
using UniVox;

namespace UnityEdits
{
    public static class ChunkSize
    {
        //We use bit-shifting to make it obvious how many bits each axis has
        //IF we require chunks to be equal; 8->2=>4, 16->5=>32, 32->10=>1024, 64->21=>BIG #

        private const int ByteAxisBits = 2;
        private const int ShortAxisBits = 5;
        private const int AxisBits = ShortAxisBits;
        public const int AxisSize = 1 << AxisBits;
        public const int SquareSize = AxisSize * AxisSize;

        public const int CubeSize = SquareSize * AxisSize;
//        public const AxisOrdering Ordering = AxisOrdering.YXZ;

        public static int3 GetChunkSize() => new int3(AxisSize);
        public static int GetIndex(int3 position) => PositionToIndexUtil.ToIndex(position, GetChunkSize());
        public static int GetIndex(int x, int y, int z) => PositionToIndexUtil.ToIndex(x, y, z, AxisSize, AxisSize);

        public static int GetIndex(int2 position) => PositionToIndexUtil.ToIndex(position, AxisSize);
        public static int GetIndex(int x, int y) => PositionToIndexUtil.ToIndex(x, y, AxisSize);
        public static int3 GetPosition3(int index) => PositionToIndexUtil.ToPosition3(index, AxisSize, AxisSize);
        public static int2 GetPosition2(int index) => PositionToIndexUtil.ToPosition2(index, AxisSize);
    }

    public static class UnivoxDefine
    {
    }

    public static class UnivoxPhysics
    {
        public static int3 ToVoxelSpace(float3 position)
        {
            return ToVoxelSpace(position, float3.zero);
        }

        private static readonly float3 VoxelSpaceOffset = new float3(1f / 2f);
        private const float ReallySmallMultiplier = 1f / 100f;

        public static int3 ToVoxelSpace(float3 position, float3 normal)
        {
            normal *= ReallySmallMultiplier;
            //Assuming no offset, your standard block will have any point be +- 0.5
            //We subtract the normal so we can move inward, towards 0.
            //We add the Voxel Space Offset, so our points become 0 to 1
            //

            var fixedPosition = position - normal; // + VoxelSpaceOffset;

            return new int3(math.floor(fixedPosition));


            //A negative -.5 should map to 0, or -1
            //Now i remember this problem
        }

        public static int3 ToWorldPosition(int3 chunkPosition, int3 blockPosition)
        {
            return chunkPosition * ChunkSize.AxisSize + blockPosition;
        }

        public static void SplitPosition(int3 worldPosition, out int3 chunkPosition, out int3 blockPosition)
        {
            chunkPosition = worldPosition / ChunkSize.AxisSize;
            blockPosition = worldPosition % ChunkSize.AxisSize;
        }

        public static Vector3 ToUnitySpace(int3 voxelPosition)
        {
            return voxelPosition + VoxelSpaceOffset;
        }
    }
}