using Unity.Mathematics;

namespace UniVox.Utility
{
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
}