using System;
using UniVox.Entities.Systems;
using UniVox.Entities.Systems.Registry;
using UniVox.Managers;

namespace UniVox.Core.Types
{
    public struct BlockIdentity : IEquatable<BlockIdentity>, IComparable<BlockIdentity>
    {
        public BlockIdentity(ModId mod, int block)
        {
            Mod = mod;
            Block = block;
        }

        public ModId Mod;

        public int Block;


//        public bool TryGetBlockReference(ModRegistry modRegistry,
//            out IAutoReference<string, BaseBlockReference> reference)
//        {
//            return modRegistry.TryGetBlockReference(Mod, Block, out reference);
//        }


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