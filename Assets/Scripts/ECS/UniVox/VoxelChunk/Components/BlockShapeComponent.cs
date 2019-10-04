using System;
using Unity.Entities;
using UniVox;
using UniVox.Types;

namespace ECS.UniVox.VoxelChunk.Components
{
    [InternalBufferCapacity(UnivoxDefine.ByteCubeSize)]
    public struct BlockShapeComponent : IBufferElementData,
        IComparable<BlockShapeComponent>, IEquatable<BlockShapeComponent>
    {
        public BlockShape Value;

        public static implicit operator BlockShape(BlockShapeComponent component)
        {
            return component.Value;
        }

        public static implicit operator BlockShapeComponent(BlockShape value)
        {
            return new BlockShapeComponent() {Value = value};
        }


        public int CompareTo(BlockShapeComponent other)
        {
            return Value - other;
        }

        public bool Equals(BlockShapeComponent other)
        {
            return Value == other;
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
                return ChangeVersionUtility.DidChange(Value,other.Value);
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