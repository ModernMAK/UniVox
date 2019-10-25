using System;

namespace UniVox.Types
{
   
    public struct BlockKey : IEquatable<BlockKey>, IComparable<BlockKey>
    {
        public BlockKey(ModKey mod, string array)
        {
            Mod = mod;
            Value = array;
        }

        public ModKey Mod { get; }
        public string Value { get; }


        public override string ToString() => $"{Mod}~{Value}";


        public bool Equals(BlockKey other) => Mod.Equals(other.Mod) && string.Equals(Value, other.Value);
        

        public override bool Equals(object obj) => obj is BlockKey other && Equals(other);
        

        public override int GetHashCode()
        {
            unchecked
            {
                return (Mod.GetHashCode() * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }

        public int CompareTo(BlockKey other)
        {
            var modComparison = Mod.CompareTo(other.Mod);
            if (modComparison != 0) return modComparison;
            return string.Compare(Value, other.Value, StringComparison.Ordinal);
        }
    }
}