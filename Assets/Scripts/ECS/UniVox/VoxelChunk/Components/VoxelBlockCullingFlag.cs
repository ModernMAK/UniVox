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

        
        public override string ToString()
        {
            return Value.ToString();
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

        public struct Version : ISystemStateComponentData, IEquatable<Version>,
            IVersionDirtyProxy<Version>
        {
            public uint Value;

            public bool Equals(Version other)
            {
                return Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                return obj is Version other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (int) Value;
            }

            public static implicit operator uint(Version version)
            {
                return version.Value;
            }

            public static implicit operator Version(uint value)
            {
                return new Version {Value = value};
            }

            public bool DidChange(Version other)
            {
                return ChangeVersionUtility.DidChange(Value, other.Value);
            }

            public Version GetDirty()
            {
                var temp = Value;
                ChangeVersionUtility.IncrementGlobalSystemVersion(ref temp);
                return new Version {Value = temp};
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }
    }
}