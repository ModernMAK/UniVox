using System;

namespace UniVox.Types
{
    public struct SubMaterialKey : IEquatable<SubMaterialKey>, IComparable<SubMaterialKey>
    {
        public SubMaterialKey(ModKey mod, string value)
        {
            Mod = mod;
            Value = value;
        }
        public SubMaterialKey(MaterialKey material, string value)
        {
            Mod = material.Mod;
            Value = $"{material.Value}~{value}";
        }

        public ModKey Mod { get; }
        public string Value { get; }


        public override string ToString() => $"{Mod}~{Value}";


        public bool Equals(SubMaterialKey other) => Mod.Equals(other.Mod) && string.Equals(Value, other.Value);
        

        public override bool Equals(object obj) => obj is SubMaterialKey other && Equals(other);
        

        public override int GetHashCode()
        {
            unchecked
            {
                return (Mod.GetHashCode() * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }

        public int CompareTo(SubMaterialKey other)
        {
            var modComparison = Mod.CompareTo(other.Mod);
            if (modComparison != 0) return modComparison;
            return string.Compare(Value, other.Value, StringComparison.Ordinal);
        }
    }
}