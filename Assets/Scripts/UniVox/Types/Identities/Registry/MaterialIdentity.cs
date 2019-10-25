using System;

namespace UniVox.Types
{
    /// <summary>
    /// Like other KEY/IDs I use this for type safety, also makes type refactoring easier.
    /// </summary>
    public struct MaterialIdentity : IEquatable<MaterialIdentity>, IComparable<MaterialIdentity>
    {
        private short Value { get; }


        public MaterialIdentity(int value)
        {
            Value = (short)value;
        }
        public MaterialIdentity(short value)
        {
            Value = value;
        }


        public override string ToString()
        {
            return $"Material:{Value:X}";
        }


        public static implicit operator int(MaterialIdentity id)
        {
            return id.Value;
        }
        public static implicit operator MaterialIdentity(int value)
        {
            return new MaterialIdentity(value);
        }
        public static implicit operator MaterialIdentity(short value)
        {
            return new MaterialIdentity(value);
        }
        public static implicit operator short(MaterialIdentity id)
        {
            return id.Value;
        }

        public int CompareTo(MaterialIdentity other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(MaterialIdentity other)
        {
            return  Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is MaterialIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}