namespace UniVox.Types
{
    public struct IconIdentity
    {
        public int Value { get; }


        public IconIdentity(int mesh)
        {
            Value = mesh;
        }

        public IconIdentity(short mesh)
        {
            Value = mesh;
        }

        public override string ToString()
        {
            return $"Value:{Value:X}";
        }

        public static implicit operator IconIdentity(int value) => new IconIdentity(value);
        public static implicit operator IconIdentity(short value) => new IconIdentity(value);

        public static implicit operator short(IconIdentity id) => (short) id.Value;

        public static implicit operator int(IconIdentity id) => id.Value;

        public int CompareTo(IconIdentity other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(IconIdentity other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is IconIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}