using System;
using Unity.Entities;
using UniVox;
using UniVox.Types;

namespace ECS.UniVox.VoxelChunk.Components
{
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct VoxelBlockSubMaterial : IBufferElementData,
        IComparable<VoxelBlockSubMaterial>, IEquatable<VoxelBlockSubMaterial>
    {
        private VoxelBlockSubMaterial(FaceSubMaterial value)
        {
            Value = value;
        }
        

        private FaceSubMaterial Value { get; }


        public int this[Direction direction] => Value[direction];

        public static implicit operator FaceSubMaterial(VoxelBlockSubMaterial component)
        {
            return component.Value;
        }

        public static implicit operator VoxelBlockSubMaterial(FaceSubMaterial value)
        {
            return new VoxelBlockSubMaterial(value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public int CompareTo(VoxelBlockSubMaterial other)
        {
            return Value.CompareTo(other);
        }

        public bool Equals(VoxelBlockSubMaterial other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelBlockSubMaterial other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public struct VersionProxyDirty : ISystemStateComponentData, IEquatable<VersionProxyDirty>,
            IVersionDirtyProxy<VersionProxyDirty>
        {
            public uint Value;

            public bool Equals(VersionProxyDirty other)
            {
                return Value == other.Value;
            }

            public override bool Equals(object obj)
            {
                return obj is VersionProxyDirty other && Equals(other);
            }

            public override int GetHashCode()
            {
                return (int) Value;
            }

            public static implicit operator uint(VersionProxyDirty versionProxyDirty)
            {
                return versionProxyDirty.Value;
            }

            public static implicit operator VersionProxyDirty(uint value)
            {
                return new VersionProxyDirty {Value = value};
            }

            public bool DidChange(VersionProxyDirty other)
            {
                return ChangeVersionUtility.DidChange(Value, other.Value);
            }

            public override string ToString()
            {
                return Value.ToString();
            }

            public VersionProxyDirty GetDirty()
            {
                var temp = Value;
                ChangeVersionUtility.IncrementGlobalSystemVersion(ref temp);
                return new VersionProxyDirty {Value = temp};
            }
        }
    }
}