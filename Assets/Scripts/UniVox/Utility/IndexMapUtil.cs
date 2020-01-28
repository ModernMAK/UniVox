using Unity.Mathematics;

namespace UniVox.Utility
{
    public interface IIndexConverter<T>
    {
        T Size { get; }
        int Flatten(T value);
        T Expand(int value);
    }

    public struct IndexConverter2D : IIndexConverter<int2>
    {
        public IndexConverter2D(int2 size)
        {
            Size = size;
        }

        public int2 Size { get; }
        public int Flatten(int x, int y) => x + y * Size.x;
        public int Flatten(int2 value) => Flatten(value.x, value.y);
        public int2 Expand(int value) => new int2(value % Size.x, value / Size.x);
    }

    public struct IndexConverter3D : IIndexConverter<int3>
    {
        public IndexConverter3D(int3 size)
        {
            Size = size;
        }

        public int3 Size { get; }
        public int Flatten(int x, int y, int z) => x + y * Size.x + z * Size.x * Size.y;
        public int Flatten(int3 value) => Flatten(value.x, value.y, value.z);

        public int3 Expand(int value) =>
            new int3(value % Size.x, (value / Size.x) % Size.y, value / (Size.x * Size.y));
    }

    public struct IndexConverter4D : IIndexConverter<int4>
    {
        public IndexConverter4D(int4 size)
        {
            Size = size;
        }

        public int4 Size { get; }

        public int Flatten(int x, int y, int z, int w) => x +
                                                          y * Size.x +
                                                          z * Size.x * Size.y +
                                                          w * Size.x * Size.y + Size.w;

        public int Flatten(int4 value) => Flatten(value.x, value.y, value.z, value.w);

        public int4 Expand(int value) =>
            new int4(value % Size.x,
                (value / Size.x) % Size.y,
                value / (Size.x * Size.y) % Size.z,
                value / (Size.x * Size.y * Size.z));
    }

    public static class IndexMapUtil
    {
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