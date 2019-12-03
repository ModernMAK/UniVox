using Unity.Mathematics;

namespace UniVox.Utility
{
    public static class IndexMapUtil
    {
        public struct MapMatrix2D
        {
            public int Size { get; }
            public int Flatten(int x, int y) => x + y * Size;
            public int Flatten(int2 value) => value.x + value.y * Size;
            public int2 Expand(int value) => new int2(value % Size, value / Size);
        }

        public struct MapMatrix3D
        {
            public int2 Size { get; }
            private int SizeX => Size.x;
            private int SizeY => Size.y;
            private int SizeXY => Size.y * Size.x;
            public int Flatten(int x, int y, int z) => x + y * SizeX + z * SizeXY;
            public int Flatten(int3 value) => value.x + value.y * SizeX + value.z * SizeXY;
            public int3 Expand(int value) => new int3(value % SizeX, (value / SizeX) % SizeY, value / SizeXY);
        }

        //2D
        public static int ToIndex(int x, int y, int xSize)
        {
            return x + y * xSize;
        }

        public static int ToIndex(int2 pos, int xSize)
        {
            return pos.x + pos.y * xSize;
        }

        public static int ToIndex(int2 pos, int2 size)
        {
            return pos.x + pos.y * size.x;
        }

        public static int2 ToPosition2(int index, int xSize)
        {
            var x = index % xSize;
            var y = index / xSize;
            return new int2(x, y);
        }

        public static int2 ToPosition2(int index, int2 size)
        {
            var x = index % size.x;
            var y = index / size.x;
            return new int2(x, y);
        }

        //3D
        public static int ToIndex(int x, int y, int z, int xSize, int ySize)
        {
            return x + y * xSize + z * xSize * ySize;
        }

        public static int ToIndex(int3 pos, int xSize, int ySize)
        {
            return pos.x + pos.y * xSize + pos.z * xSize * ySize;
        }

        public static int ToIndex(int3 pos, int2 xySize)
        {
            return pos.x + pos.y * xySize.x + pos.z * xySize.x * xySize.y;
        }

        public static int ToIndex(int3 pos, int3 size)
        {
            return pos.x + pos.y * size.x + pos.z * size.x * size.y;
        }


        public static int3 ToPosition3(int index, int xSize, int ySize)
        {
            var x = index % xSize;
            var y = index / xSize % ySize;
            var z = index / (xSize * ySize);
            return new int3(x, y, z);
        }

        public static int3 ToPosition3(int index, int2 xySize)
        {
            var x = index % xySize.x;
            var y = index / xySize.x % xySize.y;
            var z = index / (xySize.x * xySize.y);
            return new int3(x, y, z);
        }

        public static int3 ToPosition3(int index, int3 size)
        {
            var x = index % size.x;
            var y = index / size.x % size.y;
            var z = index / (size.x * size.y);
            return new int3(x, y, z);
        }

        //4D
        public static int ToIndex(int x, int y, int z, int w, int xSize, int ySize, int zSize)
        {
            return x + y * xSize + z * xSize * ySize + w * xSize * ySize * zSize;
        }

        public static int ToIndex(int4 pos, int xSize, int ySize, int zSize)
        {
            return pos.x + pos.y * xSize + pos.z * xSize * ySize + pos.w * xSize * ySize * zSize;
        }

        public static int ToIndex(int4 pos, int3 xyzSize)
        {
            return pos.x + pos.y * xyzSize.x + pos.z * xyzSize.x * xyzSize.y +
                   pos.w * xyzSize.x * xyzSize.y * xyzSize.z;
        }

        public static int ToIndex(int4 pos, int4 size)
        {
            return pos.x + pos.y * size.x + pos.z * size.x * size.y + pos.w * size.x * size.y * size.z;
        }


        public static int4 ToPosition4(int index, int xSize, int ySize, int zSize)
        {
            var x = index % xSize;
            var y = index / xSize % ySize;
            var z = index / (xSize * ySize) % zSize;
            var w = index / (xSize * ySize * zSize);
            return new int4(x, y, z, w);
        }

        public static int4 ToPosition4(int index, int3 xyzSize)
        {
            var x = index % xyzSize.x;
            var y = index / xyzSize.x % xyzSize.y;
            var z = index / (xyzSize.x * xyzSize.y) % xyzSize.z;
            var w = index / (xyzSize.x * xyzSize.y * xyzSize.z);
            return new int4(x, y, z, w);
        }

        public static int4 ToPosition4(int index, int4 size)
        {
            var x = index % size.x;
            var y = index / size.x % size.y;
            var z = index / (size.x * size.y) % size.z;
            var w = index / (size.x * size.y * size.z);
            return new int4(x, y, z, w);
        }
    }
}