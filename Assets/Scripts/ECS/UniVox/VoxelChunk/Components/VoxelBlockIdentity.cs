using System;
using Unity.Entities;
using UniVox;
using UniVox.Types.Identities;

namespace ECS.UniVox.VoxelChunk.Components
{
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct VoxelBlockIdentity : IBufferElementData,
        IComparable<VoxelBlockIdentity>, IEquatable<VoxelBlockIdentity>
    {
        public BlockIdentity Value;

        public static implicit operator BlockIdentity(VoxelBlockIdentity component)
        {
            return component.Value;
        }

        public static implicit operator VoxelBlockIdentity(BlockIdentity identity)
        {
            return new VoxelBlockIdentity {Value = identity};
        }


        public int CompareTo(VoxelBlockIdentity other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(VoxelBlockIdentity other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelBlockIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }

    public struct VoxelBlockIdentityVersion : ISystemStateComponentData, IEquatable<VoxelBlockIdentityVersion>,
        IVersionDirtyProxy<VoxelBlockIdentityVersion>
    {
        public uint Value;

        public bool Equals(VoxelBlockIdentityVersion other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelBlockIdentityVersion other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int) Value;
        }

        public static implicit operator uint(VoxelBlockIdentityVersion voxelBlockIdentityVersion)
        {
            return voxelBlockIdentityVersion.Value;
        }

        public static implicit operator VoxelBlockIdentityVersion(uint value)
        {
            return new VoxelBlockIdentityVersion {Value = value};
        }


        public bool DidChange(VoxelBlockIdentityVersion other)
        {
            return ChangeVersionUtility.DidChange(Value, other.Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public VoxelBlockIdentityVersion GetDirty()
        {
            var temp = Value;
            ChangeVersionUtility.IncrementGlobalSystemVersion(ref temp);
            return new VoxelBlockIdentityVersion {Value = temp};
        }
    }
}