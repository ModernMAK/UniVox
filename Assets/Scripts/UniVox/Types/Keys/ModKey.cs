using System;

namespace UniVox.Types
{
    public struct ModKey : IComparable<ModKey>, IEquatable<ModKey>
    {
        public ModKey(string value)
        {
            Value = value;
        }

        private string Value { get; }

        public override string ToString() => Value;

        

        public int CompareTo(ModKey other)
        {
            //TODO deal with this warning, it IS relevant for latter
            return string.Compare(Value, other.Value);
        }

        public bool Equals(ModKey other) => Value == other.Value;


        public override bool Equals(object obj) => obj is ModKey other && Equals(other);


        public override int GetHashCode() => Value.GetHashCode();

        public static implicit operator string(ModKey mey) => mey.Value;


        public static implicit operator ModKey(string value) => new ModKey(value);
    }
}