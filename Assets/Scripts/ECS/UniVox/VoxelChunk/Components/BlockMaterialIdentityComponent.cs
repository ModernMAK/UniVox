using System;
using Unity.Entities;
using UniVox;
using UniVox.Types.Identities;

namespace ECS.UniVox.VoxelChunk.Components
{
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct BlockMaterialIdentityComponent : IBufferElementData,
        IComparable<BlockMaterialIdentityComponent>, IEquatable<BlockMaterialIdentityComponent>
    {
        public ArrayMaterialIdentity Value;

        public static implicit operator ArrayMaterialIdentity(BlockMaterialIdentityComponent component)
        {
            return component.Value;
        }

        public static implicit operator BlockMaterialIdentityComponent(ArrayMaterialIdentity value)
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

            public Version GetDirty()
            {
                var temp = Value;
                ChangeVersionUtility.IncrementGlobalSystemVersion(ref temp);
                return new Version() {Value = temp};
            }

            public override string ToString()
            {
                return Value.ToString();
            }
        }
    }

    public interface IVersion
    {
        uint Value { get; }
    }

    public interface IVersionProxy<TVersion> where TVersion : IVersionProxy<TVersion>
    {
        bool DidChange(TVersion other);
        TVersion GetDirty();
    }
}