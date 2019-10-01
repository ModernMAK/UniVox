using System;

namespace UniVox.Types.Keys
{
    public struct ArrayMaterialKey : IEquatable<ArrayMaterialKey>, IComparable<ArrayMaterialKey>
    {
        public ArrayMaterialKey(ModKey mod, string array)
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

        public bool Equals(ArrayMaterialKey other)
        {
            return Mod.Equals(other.Mod) && string.Equals(ArrayMaterial, other.ArrayMaterial);
        }

        public override bool Equals(object obj)
        {
            return obj is ArrayMaterialKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Mod.GetHashCode() * 397) ^ (ArrayMaterial != null ? ArrayMaterial.GetHashCode() : 0);
            }
        }

        public int CompareTo(ArrayMaterialKey other)
        {
            var modComparison = Mod.CompareTo(other.Mod);
            if (modComparison != 0) return modComparison;
            return string.Compare(ArrayMaterial, other.ArrayMaterial, StringComparison.Ordinal);
        }
    }
    
    
}