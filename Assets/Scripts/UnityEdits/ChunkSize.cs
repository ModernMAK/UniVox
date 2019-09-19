using Types;
using Unity.Mathematics;
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
}