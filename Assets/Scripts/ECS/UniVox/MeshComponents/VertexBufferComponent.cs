using Unity.Entities;
using Unity.Mathematics;

namespace ECS.UniVox.VoxelChunk.Systems
{
    public struct VertexBufferComponent : IBufferElementData
    {
        public float3 Value;

        public static implicit operator float3(VertexBufferComponent vbc)
        {
            return vbc.Value;
        }

        public static implicit operator VertexBufferComponent(float3 value)
        {
            return new VertexBufferComponent() {Value = value};
        }
    }
}