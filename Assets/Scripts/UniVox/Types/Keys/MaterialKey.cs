using System;
using UnityEngine;

namespace UniVox.Types
{
    public struct MaterialKey : IEquatable<MaterialKey>, IComparable<MaterialKey>
    {
        public MaterialKey(ModKey mod, string array)
        {
            Mod = mod;
            ArrayMaterial = array;
        }

        public ModKey Mod;
        public string ArrayMaterial;

        public string ToString(string seperator)
        {
            return $"{Mod}{seperator}{ArrayMaterial}";
        }

        public override string ToString()
        {
            return ToString("~");
        }

        public bool Equals(MaterialKey other)
        {
            return Mod.Equals(other.Mod) && string.Equals(ArrayMaterial, other.ArrayMaterial);
        }

        public override bool Equals(object obj)
        {
            return obj is MaterialKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Mod.GetHashCode() * 397) ^ (ArrayMaterial != null ? ArrayMaterial.GetHashCode() : 0);
            }
        }

        public int CompareTo(MaterialKey other)
        {
            var modComparison = Mod.CompareTo(other.Mod);
            if (modComparison != 0) return modComparison;
            return string.Compare(ArrayMaterial, other.ArrayMaterial, StringComparison.Ordinal);
        }
    }
}