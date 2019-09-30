using System;
using UniVox.Managers.Game;
using UniVox.Types;
using UniVox.VoxelData;

namespace UniVox.Launcher
{
    public struct BlockVariant : IEquatable<BlockVariant>, IComparable<BlockVariant>
    {
        public byte Value;

        public bool Equals(BlockVariant other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is BlockVariant other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public int CompareTo(BlockVariant other)
        {
            return Value.CompareTo(other.Value);
        }
    }
    public abstract class BaseBlockReference
    {
        [Obsolete("Use BlockVariant")]
        public abstract void RenderPass(BlockAccessor blockData);
        public abstract ArrayMaterialId GetMaterial(BlockVariant blockVariant);
        public abstract FaceSubMaterial GetSubMaterial(BlockVariant blockVariant);
    }
}