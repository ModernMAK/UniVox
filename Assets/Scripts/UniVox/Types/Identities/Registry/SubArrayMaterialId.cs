using System;

namespace UniVox.Types
{
    public struct SubArrayMaterialId : IComparable<SubArrayMaterialId>, IEquatable<SubArrayMaterialId>
    {
        public SubArrayMaterialId(MaterialIdentity identity, int subMaterialId)
        {
            Material = identity;
            SubArrayMaterial = subMaterialId;
        }

        public MaterialIdentity Material;
        public int SubArrayMaterial;

        public static explicit operator SubArrayMaterialId(MaterialIdentity identity)
        {
            return new SubArrayMaterialId(identity, 0);
        }

        public static implicit operator MaterialIdentity(SubArrayMaterialId value)
        {
            return value.Material;
        }

        public static implicit operator int(SubArrayMaterialId value)
        {
            return value.SubArrayMaterial;
        }

        public int CompareTo(SubArrayMaterialId other)
        {
            var modComparison = Material.CompareTo(other.Material);
            if (modComparison != 0) return modComparison;
            return SubArrayMaterial.CompareTo(other.SubArrayMaterial);
        }

        public bool Equals(SubArrayMaterialId other)
        {
            return Material.Equals(other.Material) && SubArrayMaterial == other.SubArrayMaterial;
        }

        public override bool Equals(object obj)
        {
            return obj is SubArrayMaterialId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Material.GetHashCode() * 397) ^ SubArrayMaterial;
            }
        }
    }
}