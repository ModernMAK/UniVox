using System;
using Unity.Entities;
using UniVox.Managers.Game;

namespace UniVox.VoxelData.Chunk_Components
{
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct BlockMaterialIdentityComponent : IBufferElementData,
        IComparable<BlockMaterialIdentityComponent>, IEquatable<BlockMaterialIdentityComponent>
    {
        public ArrayMaterialId Value;

        public static implicit operator ArrayMaterialId(BlockMaterialIdentityComponent component)
        {
            return component.Value;
        }

        public static implicit operator BlockMaterialIdentityComponent(ArrayMaterialId value)
        {
            return new BlockMaterialIdentityComponent() {Value = value};
        }


        public int CompareTo(BlockMaterialIdentityComponent other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(BlockMaterialIdentityComponent other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is BlockShapeComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}