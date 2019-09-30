using System;
using Unity.Entities;

namespace UniVox.VoxelData.Chunk_Components
{
    //We cant use CubeSize because that would make the ECS chunk too big
    //WE could get around this by shrinking chunks
    //OR We could manually resize the dynamic buffer
//    [InternalBufferCapacity(UnivoxDefine.CubeSize)]
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct BlockActiveComponent : IBufferElementData,
        IComparable<BlockActiveComponent>, IEquatable<BlockActiveComponent>
    {
        public bool Value;

        public static implicit operator bool(BlockActiveComponent component)
        {
            return component.Value;
        }

        public static implicit operator BlockActiveComponent(bool value)
        {
            return new BlockActiveComponent() {Value = value};
        }


        public int CompareTo(BlockActiveComponent other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(BlockActiveComponent other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is BlockActiveComponent other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public struct Version : ISystemStateComponentData, IEquatable<Version>, IVersionProxy<Version>
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
                return new Version() {Value = value};
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
                return new Version() {Value = temp};
            }
        }
    }
}