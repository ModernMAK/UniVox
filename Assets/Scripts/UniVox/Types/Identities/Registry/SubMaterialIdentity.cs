using System;

namespace UniVox.Types
{
    /// <summary>
    /// Like other KEY/IDs I use this for type safety, also makes type refactoring easier.
    /// </summary>
    public struct SubMaterialIdentity : IEquatable<SubMaterialIdentity>, IComparable<SubMaterialIdentity>
    {
        public SubMaterialIdentity(int value)
        {
            Value = (short) value;
        }

        public SubMaterialIdentity(short value)
        {
            Value = value;
        }

        private short Value { get; }


        public override string ToString()
        {
            return $"Sub Material:{Value:X}";
        }
        
        public bool Equals(SubMaterialIdentity other) => Value == other.Value;
        public override bool Equals(object obj) => obj is SubMaterialIdentity other && Equals(other);
        public override int GetHashCode() => Value;
        public int CompareTo(SubMaterialIdentity other) => Value.CompareTo(other.Value);

        public static implicit operator short(SubMaterialIdentity id) => id.Value;
        public static implicit operator int(SubMaterialIdentity id) => id.Value;
        public static implicit operator SubMaterialIdentity(short value) => new SubMaterialIdentity(value);
        public static implicit operator SubMaterialIdentity(int value) => new SubMaterialIdentity(value);
    }
}