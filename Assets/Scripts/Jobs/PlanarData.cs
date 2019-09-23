using Types;
using Unity.Mathematics;

namespace Jobs
{
    public struct PlanarData
    {
        public int3 Position;
        public Direction Direction;
        public BlockShape Shape;
        public int2 size;
    }
}