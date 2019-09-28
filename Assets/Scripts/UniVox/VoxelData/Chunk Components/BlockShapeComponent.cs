using System;
using Unity.Entities;
using UniVox.Types;

namespace UniVox.VoxelData.Chunk_Components
{
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct BlockShapeComponent : IBufferElementData,
        IComparable<BlockShapeComponent>, IEquatable<BlockShapeComponent>
    {
        public BlockShape Value;

        public static implicit operator BlockShape(BlockShapeComponent component)
        {
            return component.Value;
        }

        public static implicit operator BlockShapeComponent(BlockShape value)
        {
            return new BlockShapeComponent() {Value = value};
        }


        public int CompareTo(BlockShapeComponent other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(BlockShapeComponent other)
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