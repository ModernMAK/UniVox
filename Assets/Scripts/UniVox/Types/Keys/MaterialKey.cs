using System;
using UnityEngine;

namespace UniVox.Types
{
   
    public struct MaterialKey : IEquatable<MaterialKey>, IComparable<MaterialKey>
    {
        public MaterialKey(ModKey mod, string array)
        {
            Mod = mod;
            Value = array;
        }

        public ModKey Mod { get; }
        public string Value { get; }


        public override string ToString() => $"{Mod}~{Value}";


        public bool Equals(MaterialKey other) => Mod.Equals(other.Mod) && string.Equals(Value, other.Value);
        

        public override bool Equals(object obj) => obj is MaterialKey other && Equals(other);
        

        public override int GetHashCode()
        {
            unchecked
            {
                return (Mod.GetHashCode() * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }

        public int CompareTo(MaterialKey other)
        {
            var modComparison = Mod.CompareTo(other.Mod);
            if (modComparison != 0) return modComparison;
            return string.Compare(Value, other.Value, StringComparison.Ordinal);
        }
    }
}