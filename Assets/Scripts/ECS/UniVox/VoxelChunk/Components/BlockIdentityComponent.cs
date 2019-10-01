using System;
using Unity.Entities;
using UniVox.Types;
using UniVox.Types.Identities;

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