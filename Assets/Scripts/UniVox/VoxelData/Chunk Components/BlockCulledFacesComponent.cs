using System;
using Unity.Entities;
using UniVox.Types;

namespace UniVox.VoxelData.Chunk_Components
{
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct BlockCulledFacesComponent : IBufferElementData, IComparable<BlockCulledFacesComponent>,
        IEquatable<BlockCulledFacesComponent>
    {
        public Directions Value;

        public static implicit operator Directions(BlockCulledFacesComponent component)
        {
            return component.Value;
        }

        public static implicit operator BlockCulledFacesComponent(Directions value)
        {
            return new BlockCulledFacesComponent() {Value = value};
        }


        public int CompareTo(BlockCulledFacesComponent other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(BlockCulledFacesComponent other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is BlockCulledFacesComponent other && Equals(other);
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
}