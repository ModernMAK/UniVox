using System;
using UniVox.Types;

namespace UniVox.Managers.Game.Accessor
{
    public struct BlockKey : IEquatable<BlockKey>, IComparable<BlockKey>
    {
        public BlockKey(ModKey mod, string block)
        {
            Mod = mod;
            Block = block;
        }

        public string ToString(string seperator)
        {
            return $"{Mod}{seperator}{Block}";
        }

        public override string ToString()
        {
            return ToString("~");
        }

        public ModKey Mod;
        public string Block;

        public bool Equals(BlockKey other)
        {
            return Mod.Equals(other.Mod) && string.Equals(Block, other.Block);
        }

        public override bool Equals(object obj)
        {
            return obj is BlockKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Mod.GetHashCode() * 397) ^ (Block != null ? Block.GetHashCode() : 0);
            }
        }

        public int CompareTo(BlockKey other)
        {
            var modComparison = Mod.CompareTo(other.Mod);
            if (modComparison != 0) return modComparison;
            return string.Compare(Block, other.Block, StringComparison.Ordinal);
        }
    }
}