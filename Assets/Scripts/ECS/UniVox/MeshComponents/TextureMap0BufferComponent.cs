using Unity.Entities;
using Unity.Mathematics;

namespace ECS.UniVox.VoxelChunk.Systems
{
    public struct TextureMap0BufferComponent : IBufferElementData
    {
        public float4 Value;

        public float4 xyzw
        {
            get => Value;
            set => Value = value;
        }

        public float3 xyz
        {
            get => Value.xyz;
            set => Value = new float4(value.x, value.y, value.z, default);
        }

        public float2 xy
        {
            get => Value.xy;
            set => Value = new float4(value.x, value.y, default, default);
        }
        public float x
        {
            get => Value.x;
            set => Value = new float4(value, default, default, default);
        }

        public static implicit operator float4(TextureMap0BufferComponent vbc)
        {
            return vbc.Value;
        }

        public static implicit operator TextureMap0BufferComponent(float4 value)
        {
            return new TextureMap0BufferComponent() {Value = value};
        }
        
        public static explicit operator float3(TextureMap0BufferComponent vbc)
        {
            return vbc.xyz;
        }

        public static explicit operator TextureMap0BufferComponent(float3 value)
        {
            return new TextureMap0BufferComponent() {xyz = value};
        }
        public static explicit operator float2(TextureMap0BufferComponent vbc)
        {
            return vbc.xy;
        }

        public static explicit operator TextureMap0BufferComponent(float2 value)
        {
            return new TextureMap0BufferComponent() {xy = value};
        }
        public static explicit operator float(TextureMap0BufferComponent vbc)
        {
            return vbc.x;
        }

        public static explicit operator TextureMap0BufferComponent(float value)
        {
            return new TextureMap0BufferComponent() {xy = value};
        }
    }
}