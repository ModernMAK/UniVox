using System;

namespace UniVox.Types
{
    /// <summary>
    /// Like other KEY/IDs I use this for type safety, also makes type refactoring easier.
    /// </summary>
    public struct MaterialIdentity : IEquatable<MaterialIdentity>, IComparable<MaterialIdentity>
    {
        public MaterialIdentity(int value)
        {
            Value = (short) value;
        }

        public MaterialIdentity(short value)
        {
            Value = value;
        }

        private short Value { get; }


        public override string ToString()
        {
            return $"Material:{Value:X}";
        }
        
        public bool Equals(MaterialIdentity other) => Value == other.Value;
        public override bool Equals(object obj) => obj is MaterialIdentity other && Equals(other);
        public override int GetHashCode() => Value;
        public int CompareTo(MaterialIdentity other) => Value.CompareTo(other.Value);

        public static implicit operator short(MaterialIdentity id) => id.Value;
        public static implicit operator int(MaterialIdentity id) => id.Value;
        public static implicit operator MaterialIdentity(short value) => new MaterialIdentity(value);
        public static implicit operator MaterialIdentity(int value) => new MaterialIdentity(value);
    }
}