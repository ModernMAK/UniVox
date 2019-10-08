using System;
using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.Systems
{
    [Obsolete]
    public struct BlockActiveVersion : ISystemStateComponentData, IEquatable<BlockActiveVersion>,
        IVersionDirtyProxy<BlockActiveVersion>, IVersionProxy<BlockActiveVersion>
    {
        public BlockActiveVersion(uint value)
        {
            Value = value;
        }

        private uint Value { get; }

        public bool Equals(BlockActiveVersion other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is BlockActiveVersion other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int) Value;
        }

        public static implicit operator uint(BlockActiveVersion blockActiveVersion)
        {
            return blockActiveVersion.Value;
        }

        public static implicit operator BlockActiveVersion(uint value)
        {
            return new BlockActiveVersion(value);
        }

        public bool DidChange(BlockActiveVersion other)
        {
            return ChangeVersionUtility.DidChange(Value, other.Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public BlockActiveVersion GetDirty()
        {
            var temp = Value;
            ChangeVersionUtility.IncrementGlobalSystemVersion(ref temp);
            return new BlockActiveVersion(temp);
        }
    }
}