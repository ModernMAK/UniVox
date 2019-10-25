using System;

namespace UniVox.Types
{
    public struct MeshIdentity : IComparable<MeshIdentity>, IEquatable<MeshIdentity>
    {
    
        public MeshIdentity(int value)
        {
            Value = (short) value;
        }

        public MeshIdentity(short value)
        {
            Value = value;
        }

        private short Value { get; }


        public override string ToString()
        {
            return $"Mesh:{Value:X}";
        }
        
        public bool Equals(MeshIdentity other) => Value == other.Value;
        public override bool Equals(object obj) => obj is MeshIdentity other && Equals(other);
        public override int GetHashCode() => Value;
        public int CompareTo(MeshIdentity other) => Value.CompareTo(other.Value);

        public static implicit operator short(MeshIdentity id) => id.Value;
        public static implicit operator int(MeshIdentity id) => id.Value;
        public static implicit operator MeshIdentity(short value) => new MeshIdentity(value);
        public static implicit operator MeshIdentity(int value) => new MeshIdentity(value);
    }
}