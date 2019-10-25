using System;

namespace UniVox.Types
{
    /// <summary>
    /// Like other KEY/IDs I use this for type safety, also makes type refactoring easier.
    /// </summary>
    public struct SpriteIdentity : IEquatable<SpriteIdentity>, IComparable<SpriteIdentity>
    {
        public SpriteIdentity(int value)
        {
            Value = (short) value;
        }

        public SpriteIdentity(short value)
        {
            Value = value;
        }

        private short Value { get; }


        public override string ToString()
        {
            return $"Sprite:{Value:X}";
        }

        public bool Equals(SpriteIdentity other) => Value == other.Value;
        public override bool Equals(object obj) => obj is SpriteIdentity other && Equals(other);
        public override int GetHashCode() => Value;
        public int CompareTo(SpriteIdentity other) => Value.CompareTo(other.Value);

        public static implicit operator short(SpriteIdentity id) => id.Value;
        public static implicit operator int(SpriteIdentity id) => id.Value;
        public static implicit operator SpriteIdentity(short value) => new SpriteIdentity(value);
        public static implicit operator SpriteIdentity(int value) => new SpriteIdentity(value);
    }
}