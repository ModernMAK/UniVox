using System;

namespace UniVox.Managers.Game
{
    public struct SubArrayMaterialId : IComparable<SubArrayMaterialId>, IEquatable<SubArrayMaterialId>
    {
        public SubArrayMaterialId(ArrayMaterialId id, int subMaterialId)
        {
            ArrayMaterial = id;
            SubArrayMaterial = subMaterialId;
        }

        public ArrayMaterialId ArrayMaterial;
        public int SubArrayMaterial;

        public static explicit operator SubArrayMaterialId(ArrayMaterialId id)
        {
            return new SubArrayMaterialId(id, 0);
        }

        public static implicit operator ArrayMaterialId(SubArrayMaterialId value)
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