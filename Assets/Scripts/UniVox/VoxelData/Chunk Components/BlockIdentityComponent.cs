using System;
using Unity.Entities;
using UniVox.Types;

namespace UniVox.VoxelData.Chunk_Components
{
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct BlockIdentityComponent : IBufferElementData,
        IComparable<BlockIdentityComponent>, IEquatable<BlockIdentityComponent>
    {
        public BlockIdentity Value;

        public static implicit operator BlockIdentity(BlockIdentityComponent component)
        {
            return component.Value;
        }

        public static implicit operator BlockIdentityComponent(BlockIdentity identity)
        {
            return new BlockIdentityComponent() {Value = identity};
        }


        public int CompareTo(BlockIdentityComponent other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(BlockIdentityComponent other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is BlockIdentityComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}