using System;

namespace UniVox.Types.Keys
{
    public struct ModKey : IComparable<ModKey>, IEquatable<ModKey>
    {
        public ModKey(string value)
        {
            Value = value;
        }

        public string Value;

        public override string ToString()
        {
            return Value;
        }

        public int CompareTo(ModKey other)
        {
            //TODO deal with this warning, it IS relevant for latter
            return Value.CompareTo(other.Value);
        }

        public bool Equals(ModKey other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is ModKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator string(ModKey mey)
        {
            return mey.Value;
        }

        public static implicit operator ModKey(string value)
        {
            return new ModKey(value);
        }
    }
}