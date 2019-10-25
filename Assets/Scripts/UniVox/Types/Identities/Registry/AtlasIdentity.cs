using System;

namespace UniVox.Types
{
    public struct AtlasIdentity : IComparable<AtlasIdentity>, IEquatable<AtlasIdentity>
    {
    
        public AtlasIdentity(int value)
        {
            Value = (short) value;
        }

        public AtlasIdentity(short value)
        {
            Value = value;
        }

        private short Value { get; }


        public override string ToString()
        {
            return $"Atlas:{Value:X}";
        }
        
        public bool Equals(AtlasIdentity other) => Value == other.Value;
        public override bool Equals(object obj) => obj is AtlasIdentity other && Equals(other);
        public override int GetHashCode() => Value;
        public int CompareTo(AtlasIdentity other) => Value.CompareTo(other.Value);

        public static implicit operator short(AtlasIdentity id) => id.Value;
        public static implicit operator int(AtlasIdentity id) => id.Value;
        public static implicit operator AtlasIdentity(short value) => new AtlasIdentity(value);
        public static implicit operator AtlasIdentity(int value) => new AtlasIdentity(value);
    }
}