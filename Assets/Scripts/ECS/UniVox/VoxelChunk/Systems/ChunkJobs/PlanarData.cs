using Unity.Mathematics;
using UniVox.Types;

namespace ECS.UniVox.Systems
{
    public struct PlanarData
    {
        public int3 Position;
        public Direction Direction;
        public BlockShape Shape;
        public int2 Size;
        public int SubMaterial;
    }
}