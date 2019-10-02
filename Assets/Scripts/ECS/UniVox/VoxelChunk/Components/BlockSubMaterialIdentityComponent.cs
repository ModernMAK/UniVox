using System;
using Unity.Entities;
using UniVox;
using UniVox.VoxelData;

namespace ECS.UniVox.VoxelChunk.Components
{
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct BlockSubMaterialIdentityComponent : IBufferElementData,
        IComparable<BlockSubMaterialIdentityComponent>, IEquatable<BlockSubMaterialIdentityComponent>
    {
        public FaceSubMaterial Value;

        public static implicit operator FaceSubMaterial(BlockSubMaterialIdentityComponent component)
        {
            return component.Value;
        }

        public static implicit operator BlockSubMaterialIdentityComponent(FaceSubMaterial value)
        {
            return new BlockSubMaterialIdentityComponent() {Value = value};
        }


        public int CompareTo(BlockSubMaterialIdentityComponent other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(BlockSubMaterialIdentityComponent other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is BlockSubMaterialIdentityComponent other && Equals(other);
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