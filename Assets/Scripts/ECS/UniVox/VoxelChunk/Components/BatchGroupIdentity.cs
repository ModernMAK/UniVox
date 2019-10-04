using System;
using UniVox.Types.Identities;
using UniVox.Types.Identities.Voxel;

namespace ECS.UniVox.VoxelChunk.Components
{
    public struct BatchGroupIdentity : IEquatable<BatchGroupIdentity>, IComparable<BatchGroupIdentity>
    {
        public ChunkIdentity Chunk;
        public ArrayMaterialIdentity MaterialIdentity;

        public bool Equals(BatchGroupIdentity other)
        {
            return Chunk.Equals(other.Chunk) && MaterialIdentity.Equals(other.MaterialIdentity);
        }

        public override bool Equals(object obj)
        {
            return obj is BatchGroupIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Chunk.GetHashCode() * 397) ^ MaterialIdentity.GetHashCode();
            }
        }

        public int CompareTo(BatchGroupIdentity other)
        {
            var chunk = Chunk.CompareTo(other.Chunk);
            return chunk != 0 ? chunk : MaterialIdentity.CompareTo(other.MaterialIdentity);
        }

        public override string ToString()
        {
            return $"Chunk:({Chunk}), Material:({MaterialIdentity})";
        }
    }
}