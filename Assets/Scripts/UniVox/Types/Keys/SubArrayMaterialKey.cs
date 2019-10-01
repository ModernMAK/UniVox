using System;

namespace UniVox.Types.Keys
{
    public struct SubArrayMaterialKey : IEquatable<SubArrayMaterialKey>, IComparable<SubArrayMaterialKey>
    {
        public SubArrayMaterialKey(ArrayMaterialKey arrayMaterial, string subArrayMaterial)
        {
            ArrayMaterial = arrayMaterial;
            SubArrayMaterial = subArrayMaterial;
        }

//        public ModKey Mod => Icon.Mod;
        public ArrayMaterialKey ArrayMaterial;
        public string SubArrayMaterial;

        public string ToString(string seperator)
        {
            return $"{ArrayMaterial.ToString(seperator)}{seperator}{ArrayMaterial}";
        }

        public override string ToString()
        {
            return ToString("~");
        }

        public bool Equals(SubArrayMaterialKey other)
        {
            return ArrayMaterial.Equals(other.ArrayMaterial) && string.Equals(SubArrayMaterial, other.SubArrayMaterial);
        }

        public override bool Equals(object obj)
        {
            return obj is SubArrayMaterialKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ArrayMaterial.GetHashCode() * 397) ^
                       (SubArrayMaterial != null ? SubArrayMaterial.GetHashCode() : 0);
            }
        }

        public int CompareTo(SubArrayMaterialKey other)
        {
            var materialComparison = ArrayMaterial.CompareTo(other.ArrayMaterial);
            if (materialComparison != 0) return materialComparison;
            return string.Compare(SubArrayMaterial, other.SubArrayMaterial, StringComparison.Ordinal);
        }
    }
}