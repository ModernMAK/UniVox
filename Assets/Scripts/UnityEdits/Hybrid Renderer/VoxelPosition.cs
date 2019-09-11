using Unity.Entities;

namespace UnityEdits.Rendering
{
    struct VoxelPosition : IComponentData
    {
        public short Index;

        public int X => Index % ChunkSize.AxisSize;
        public int Y => (Index / ChunkSize.AxisSize) % ChunkSize.AxisSize;
        public int Z => Index / ChunkSize.SquareSize;

        public bool ValidIndex =>
            Index < ChunkSize.CubeSize && Index >= 0; //Could also use bit twiddling, but this is readable
    }
}