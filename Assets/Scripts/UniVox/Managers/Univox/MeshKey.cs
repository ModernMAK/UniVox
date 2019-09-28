using System;

namespace UniVox.Entities.Systems.Registry
{
    public struct MeshKey : IEquatable<MeshKey>, IComparable<MeshKey>
    {
        public MeshKey(ModKey mod, string mesh)
        {
            Mod = mod;
            Mesh = mesh;
        }
        
        //Why not just use string? IF i ever change it, changes will propgate across types
        //Admittedly, I can't imagine ever changing it
        public ModKey Mod;

        public string Mesh;
        
//        
//        public static explicit operator string(MeshKey mey)
//        {
//            return mey.Mesh;
//        }

        public static implicit operator MeshKey(ModKey value)
        {
            return new MeshKey(value,"");
        }
        public static implicit operator ModKey(MeshKey value)
        {
            return value.Mod;
        }

        public int CompareTo(MeshKey other)
        {
            var modComparison = Mod.CompareTo(other.Mod);
            if (modComparison != 0) return modComparison;
            return string.Compare(Mesh, other.Mesh, StringComparison.Ordinal);
        }

        public bool Equals(MeshKey other)
        {
            return Mod.Equals(other.Mod) && string.Equals(Mesh, other.Mesh);
        }

        public override bool Equals(object obj)
        {
            return obj is MeshKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Mod.GetHashCode() * 397) ^ (Mesh != null ? Mesh.GetHashCode() : 0);
            }
        }
    }
}