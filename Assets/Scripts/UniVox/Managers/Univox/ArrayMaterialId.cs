namespace UniVox.Entities.Systems.Registry
{
    public struct ArrayMaterialId
    {
        public ModId Mod;
        public int ArrayMaterial;
        
        
        public ArrayMaterialId(ModId id, int mesh)
        {
            Mod = id;
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

        public static explicit operator ArrayMaterialId(ModId id)
        {
            return new ArrayMaterialId(id, 0);
        }

        public static implicit operator ModId(ArrayMaterialId value)
        {
            return value.Mod;
        }

        public static implicit operator int(ArrayMaterialId value)
        {
            return value.ArrayMaterial;
        }

        public int CompareTo(ArrayMaterialId other)
        {
            var modComparison = Mod.CompareTo(other.Mod);
            if (modComparison != 0) return modComparison;
            return ArrayMaterial.CompareTo(other.ArrayMaterial);
        }

        public bool Equals(ArrayMaterialId other)
        {
            return Mod.Equals(other.Mod) && ArrayMaterial == other.ArrayMaterial;
        }

        public override bool Equals(object obj)
        {
            return obj is ArrayMaterialId other && Equals(other);
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