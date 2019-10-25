using System;

namespace UniVox.Types
{
    public struct SubArrayMaterialKey : IEquatable<SubArrayMaterialKey>, IComparable<SubArrayMaterialKey>
    {
        public SubArrayMaterialKey(MaterialKey material, string subArrayMaterial)
        {
            Material = material;
            SubArrayMaterial = subArrayMaterial;
        }

//        public ModKey Mod => Value.Mod;
        public MaterialKey Material;
        public string SubArrayMaterial;

        public string ToString(string seperator)
        {
            return $"{Material.ToString(seperator)}{seperator}{SubArrayMaterial}";
        }

        public override string ToString()
        {
            return ToString("~");
        }

        public bool Equals(SubArrayMaterialKey other)
        {
            return Material.Equals(other.Material) && string.Equals(SubArrayMaterial, other.SubArrayMaterial);
        }

        public override bool Equals(object obj)
        {
            return obj is SubArrayMaterialKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Material.GetHashCode() * 397) ^
                       (SubArrayMaterial != null ? SubArrayMaterial.GetHashCode() : 0);
            }
        }

        public int CompareTo(SubArrayMaterialKey other)
        {
            var materialComparison = Material.CompareTo(other.Material);
            if (materialComparison != 0) return materialComparison;
            return string.Compare(SubArrayMaterial, other.SubArrayMaterial, StringComparison.Ordinal);
        }
    }
}