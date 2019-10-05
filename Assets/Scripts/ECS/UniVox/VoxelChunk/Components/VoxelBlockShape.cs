using System;
using Unity.Entities;
using UniVox;
using UniVox.Types;

namespace ECS.UniVox.VoxelChunk.Components
{
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct VoxelBlockShape : IBufferElementData,
        IComparable<VoxelBlockShape>, IEquatable<VoxelBlockShape>
    {
        private VoxelBlockShape(BlockShape value)
        {
            Value = value;
        }

        private BlockShape Value { get; }

        public static implicit operator BlockShape(VoxelBlockShape component)
        {
            return component.Value;
        }

        public static implicit operator VoxelBlockShape(BlockShape value)
        {
            return new VoxelBlockShape(value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public int CompareTo(VoxelBlockShape other)
        {
            return Value - other;
        }

        public bool Equals(VoxelBlockShape other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelBlockShape other && Equals(other);
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

            public override string ToString()
            {
                return Value.ToString();
            }

            public Version GetDirty()
            {
                var temp = Value;
                ChangeVersionUtility.IncrementGlobalSystemVersion(ref temp);
                return new Version {Value = temp};
            }
        }
    }
}