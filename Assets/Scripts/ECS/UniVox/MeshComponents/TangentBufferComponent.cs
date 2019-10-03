using Unity.Entities;
using Unity.Mathematics;

namespace ECS.UniVox.VoxelChunk.Systems
{
    public struct TangentBufferComponent : IBufferElementData
    {
        public float4 Value;

        public static implicit operator float4(TangentBufferComponent vbc)
        {
            return vbc.Value;
        }

        public static implicit operator TangentBufferComponent(float4 value)
        {
            return new TangentBufferComponent {Value = value};
        }
    }
}