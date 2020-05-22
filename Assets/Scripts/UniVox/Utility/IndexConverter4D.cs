using Unity.Mathematics;

namespace UniVox.Utility
{
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
}