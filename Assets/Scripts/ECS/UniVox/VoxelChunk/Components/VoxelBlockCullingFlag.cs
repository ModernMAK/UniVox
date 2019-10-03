using System;
using Unity.Entities;
using UniVox;
using UniVox.Types;

namespace ECS.UniVox.VoxelChunk.Components
{
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct VoxelBlockCullingFlag : IBufferElementData, IComparable<VoxelBlockCullingFlag>,
        IEquatable<VoxelBlockCullingFlag>
    {
        public VoxelBlockCullingFlag(Directions value)
        {
            Value = value;
        }

        private Directions Value { get; }

        public static implicit operator Directions(VoxelBlockCullingFlag component)
        {
            return component.Value;
        }

        public static implicit operator VoxelBlockCullingFlag(Directions value)
        {
            return new VoxelBlockCullingFlag(value);
        }

        /// <summary>
        ///     Checks if the direction is culled.
        /// </summary>
        /// <param name="direction">The direction to check.</param>
        /// <returns>True if the direction is culled, false otherwise.</returns>
        public bool IsCulled(Direction direction)
        {
            return Value.HasDirection(direction);
        }


        public int CompareTo(VoxelBlockCullingFlag other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(VoxelBlockCullingFlag other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelBlockCullingFlag other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public struct BlockCullFlagVersion : ISystemStateComponentData, IEquatable<BlockCullFlagVersion>,
            IVersionDirtyProxy<BlockCullFlagVersion>
        {
            public uint Value;

            public bool Equals(BlockCullFlagVersion other)
            {
                return Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                return obj is BlockCullFlagVersion other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (int) Value;
            }

            public static implicit operator uint(BlockCullFlagVersion blockCullFlagVersion)
            {
                return blockCullFlagVersion.Value;
            }

            public static implicit operator BlockCullFlagVersion(uint value)
            {
                return new BlockCullFlagVersion {Value = value};
            }

            public bool DidChange(BlockCullFlagVersion other)
            {
                return ChangeVersionUtility.DidChange(Value, other.Value);
            }

            public BlockCullFlagVersion GetDirty()
            {
                var temp = Value;
                ChangeVersionUtility.IncrementGlobalSystemVersion(ref temp);
                return new BlockCullFlagVersion {Value = temp};
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }
    }
}