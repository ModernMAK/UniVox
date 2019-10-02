using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    public struct IndexBufferComponent : IBufferElementData
    {
        public int Value;

        public static implicit operator int(IndexBufferComponent vbc)
        {
            return vbc.Value;
        }

        public static implicit operator IndexBufferComponent(int value)
        {
            return new IndexBufferComponent() {Value = value};
        }
    }
}