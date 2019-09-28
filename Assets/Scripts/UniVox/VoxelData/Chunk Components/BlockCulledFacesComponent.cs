using System;
using Unity.Entities;
using UniVox.Types;

namespace UniVox.VoxelData.Chunk_Components
{
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct BlockCulledFacesComponent : IBufferElementData,
        IComparable<BlockCulledFacesComponent>, IEquatable<BlockCulledFacesComponent>
    {
        public Directions Value;

        public static implicit operator Directions(BlockCulledFacesComponent component)
        {
            return component.Value;
        }

        public static implicit operator BlockCulledFacesComponent(Directions value)
        {
            return new BlockCulledFacesComponent() {Value = value};
        }


        public int CompareTo(BlockCulledFacesComponent other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(BlockCulledFacesComponent other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is BlockCulledFacesComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}