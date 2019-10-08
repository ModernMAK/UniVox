using System;

namespace UniVox.Types
{
    public struct SubArrayMaterialId : IComparable<SubArrayMaterialId>, IEquatable<SubArrayMaterialId>
    {
        public SubArrayMaterialId(ArrayMaterialIdentity identity, int subMaterialId)
        {
            ArrayMaterial = identity;
            SubArrayMaterial = subMaterialId;
        }

        public ArrayMaterialIdentity ArrayMaterial;
        public int SubArrayMaterial;

        public static explicit operator SubArrayMaterialId(ArrayMaterialIdentity identity)
        {
            return new SubArrayMaterialId(identity, 0);
        }

        public static implicit operator ArrayMaterialIdentity(SubArrayMaterialId value)
        {
            return value.ArrayMaterial;
        }

        public static implicit operator int(SubArrayMaterialId value)
        {
            return value.SubArrayMaterial;
        }

        public int CompareTo(SubArrayMaterialId other)
        {
            var modComparison = ArrayMaterial.CompareTo(other.ArrayMaterial);
            if (modComparison != 0) return modComparison;
            return SubArrayMaterial.CompareTo(other.SubArrayMaterial);
        }

        public bool Equals(SubArrayMaterialId other)
        {
            return ArrayMaterial.Equals(other.ArrayMaterial) && SubArrayMaterial == other.SubArrayMaterial;
        }

        public override bool Equals(object obj)
        {
            return obj is SubArrayMaterialId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ArrayMaterial.GetHashCode() * 397) ^ SubArrayMaterial;
            }
        }
    }
}