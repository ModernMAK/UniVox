using System;

namespace UniVox.Types.Identities
{
    public struct BlockIdentity : IEquatable<BlockIdentity>, IComparable<BlockIdentity>
    {
        public BlockIdentity(ModIdentity mod, int block)
        {
            Mod = mod;
            Block = block;
        }

        public ModIdentity Mod;

        public int Block;


        public string ToString(string seperator)
        {
            return $"{Mod}{seperator}{Block}";
        }

        public override string ToString()
        {
            return ToString("~");
        }

        public bool Equals(BlockIdentity other)
        {
            return Mod == other.Mod && Block == other.Block;
        }

        public override bool Equals(object obj)
        {
            return obj is BlockIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Mod * 397) ^ Block;
            }
        }

        public int CompareTo(BlockIdentity other)
        {
            var modComparison = Mod.CompareTo(other.Mod);
            if (modComparison != 0) return modComparison;
            return Block.CompareTo(other.Block);
        }

    }
}