using System;
using Unity.Entities;

namespace UniVox.Core.Types
{
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct BlockSubMaterialIdentityComponent : IBufferElementData,
        IComparable<BlockSubMaterialIdentityComponent>, IEquatable<BlockSubMaterialIdentityComponent>
    {
        public FaceSubMaterial Value;

        public static implicit operator FaceSubMaterial(BlockSubMaterialIdentityComponent component)
        {
            return component.Value;
        }

        public static implicit operator BlockSubMaterialIdentityComponent(FaceSubMaterial value)
        {
            return new BlockSubMaterialIdentityComponent() {Value = value};
        }


        public int CompareTo(BlockSubMaterialIdentityComponent other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(BlockSubMaterialIdentityComponent other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is BlockSubMaterialIdentityComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}