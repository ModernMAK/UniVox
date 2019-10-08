namespace UniVox.Types
{
    public struct IconIdentity
    {
        public ModIdentity Mod;
        public int Icon;


        public IconIdentity(ModIdentity identity, int mesh)
        {
            Mod = identity;
            Icon = mesh;
        }

        public override string ToString()
        {
            return $"Mod:({Mod}), Icon:({Icon})";
        }

        public static explicit operator IconIdentity(ModIdentity identity)
        {
            return new IconIdentity(identity, 0);
        }

        public static implicit operator ModIdentity(IconIdentity value)
        {
            return value.Mod;
        }

        public static implicit operator int(IconIdentity value)
        {
            return value.Icon;
        }

        public int CompareTo(IconIdentity other)
        {
            var modComparison = Mod.CompareTo(other.Mod);
            if (modComparison != 0) return modComparison;
            return Icon.CompareTo(other.Icon);
        }

        public bool Equals(IconIdentity other)
        {
            return Mod.Equals(other.Mod) && Icon == other.Icon;
        }

        public override bool Equals(object obj)
        {
            return obj is IconIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Mod.GetHashCode() * 397) ^ Icon;
            }
        }
    }
}