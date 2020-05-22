using Unity.Mathematics;

namespace UniVox.Utility
{
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
}