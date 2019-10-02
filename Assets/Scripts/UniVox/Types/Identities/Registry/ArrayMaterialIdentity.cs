using System;
using Unity.Entities;

namespace UniVox.Types.Identities
{
    public struct ArrayMaterialIdentity : IEquatable<ArrayMaterialIdentity>, IComparable<ArrayMaterialIdentity>
    {
        public ModIdentity Mod;
        public int ArrayMaterial;


        public ArrayMaterialIdentity(ModIdentity identity, int mesh)
        {
            Mod = identity;
            ArrayMaterial = mesh;
        }

        public string ToString(string seperator)
        {
            return $"{Mod}{seperator}{ArrayMaterial}";
        }

        public override string ToString()
        {
            return ToString("~");
        }

        public static explicit operator ArrayMaterialIdentity(ModIdentity identity)
        {
            return new ArrayMaterialIdentity(identity, 0);
        }

        public static implicit operator ModIdentity(ArrayMaterialIdentity value)
        {
            return value.Mod;
        }

        public static implicit operator int(ArrayMaterialIdentity value)
        {
            return value.ArrayMaterial;
        }

        public int CompareTo(ArrayMaterialIdentity other)
        {
            var modComparison = Mod.CompareTo(other.Mod);
            if (modComparison != 0) return modComparison;
            return ArrayMaterial.CompareTo(other.ArrayMaterial);
        }

        public bool Equals(ArrayMaterialIdentity other)
        {
            return Mod.Equals(other.Mod) && ArrayMaterial == other.ArrayMaterial;
        }

        public override bool Equals(object obj)
        {
            return obj is ArrayMaterialIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Mod.GetHashCode() * 397) ^ ArrayMaterial;
            }
        }
    }
}