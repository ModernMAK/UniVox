using System;

namespace UniVox.Types
{
    /// <summary>
    /// Like other KEY/IDs I use this for type safety, also makes type refactoring easier.
    /// </summary>
    public struct SpriteIdentity : IEquatable<SpriteIdentity>, IComparable<SpriteIdentity>
    {
        private short Value { get; }


        public SpriteIdentity(int value)
        {
            Value = (short)value;
        }
        public SpriteIdentity(short value)
        {
            Value = value;
        }


        public override string ToString()
        {
            return $"Sprite:{Value:X}";
        }


        public static implicit operator int(SpriteIdentity id)
        {
            return id.Value;
        }
        public static implicit operator SpriteIdentity(int value)
        {
            return new SpriteIdentity(value);
        }
        public static implicit operator SpriteIdentity(short value)
        {
            return new SpriteIdentity(value);
        }
        public static implicit operator short(SpriteIdentity id)
        {
            return id.Value;
        }

        public int CompareTo(SpriteIdentity other)
        {
            return Value.CompareTo(other.Value);
        }

        public bool Equals(SpriteIdentity other)
        {
            return  Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is SpriteIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}