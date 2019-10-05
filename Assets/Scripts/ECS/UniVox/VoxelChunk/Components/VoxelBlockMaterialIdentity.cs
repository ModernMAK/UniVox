using System;
using Unity.Entities;
using UniVox;
using UniVox.Types.Identities;

namespace ECS.UniVox.VoxelChunk.Components
{
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct VoxelBlockMaterialIdentity : IBufferElementData,
        IComparable<VoxelBlockMaterialIdentity>, IEquatable<VoxelBlockMaterialIdentity>
    {
        public ArrayMaterialIdentity Value;

        public static implicit operator ArrayMaterialIdentity(VoxelBlockMaterialIdentity component)
        {
            return component.Value;
        }

        public static implicit operator VoxelBlockMaterialIdentity(ArrayMaterialIdentity value)
        {
            return new VoxelBlockMaterialIdentity {Value = value};
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public int CompareTo(VoxelBlockMaterialIdentity other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(VoxelBlockMaterialIdentity other)
        {
            return Value.Equals(other.Value);
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

    //This does not implement IVersion, as we assume that Proxies contain multiple values


    //This does not implement IVersion, as we assume that Proxies contain multiple values
}