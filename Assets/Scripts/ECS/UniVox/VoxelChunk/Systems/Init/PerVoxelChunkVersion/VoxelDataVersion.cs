using System;
using ECS.UniVox.VoxelChunk.Components;
using Unity.Entities;

namespace ECS.UniVox.VoxelChunk.Systems
{
    public struct VoxelDataVersion : ISystemStateComponentData, IEquatable<VoxelDataVersion>,
        IVersionDirtyProxy<VoxelDataVersion>, IVersionProxy<VoxelDataVersion>
    {
        public VoxelDataVersion(uint value)
        {
            Value = value;
        }

        private uint Value { get; }

        public bool Equals(VoxelDataVersion other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelDataVersion other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int) Value;
        }

        public static implicit operator uint(VoxelDataVersion blockActiveVersion)
        {
            return blockActiveVersion.Value;
        }

        public static implicit operator VoxelDataVersion(uint value)
        {
            return new VoxelDataVersion(value);
        }

        public bool DidChange(VoxelDataVersion other)
        {
            return ChangeVersionUtility.DidChange(Value, other.Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public VoxelDataVersion GetDirty()
        {
            var temp = Value;
            ChangeVersionUtility.IncrementGlobalSystemVersion(ref temp);
            return new VoxelDataVersion(temp);
        }
    }
}