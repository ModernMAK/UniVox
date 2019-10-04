using Unity.Entities;
using Unity.Mathematics;

namespace ECS.UniVox.VoxelChunk.Systems
{
    public struct NormalBufferComponent : IBufferElementData
    {
        public float3 Value;

        public static implicit operator float3(NormalBufferComponent vbc)
        {
            return vbc.Value;
        }

        public static implicit operator NormalBufferComponent(float3 value)
        {
            return new NormalBufferComponent {Value = value};
        }
    }
}