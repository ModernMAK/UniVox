using System;
using Unity.Entities;
using UniVox.Types.Identities.Voxel;
using UniVox.VoxelData;
using World = UniVox.VoxelData.World;

namespace ECS.UniVox.VoxelChunk.Components
{
    public struct VoxelChunkIdentity : IComponentData, IEquatable<VoxelChunkIdentity>, IComparable<VoxelChunkIdentity>
    {
        /*
         * A note because i keep thinking i need to do this. BECAUSE entities are stored on a per World Basis, we dont need to use a Shared Component To GRoup Them
         * We do still need the WorldID since we dont store a reference to the World (Even though we know the entity knows the EntityWorld their in)
         */
        public ChunkIdentity Value;


        public bool Equals(VoxelChunkIdentity other)
        {
            return Value.Equals(other.Value);
        }

        public int CompareTo(VoxelChunkIdentity other)
        {
            return Value.CompareTo(other.Value);
        }

        public static implicit operator ChunkIdentity(VoxelChunkIdentity component)
        {
            return component.Value;
        }

        public static implicit operator VoxelChunkIdentity(ChunkIdentity value)
        {
            return new VoxelChunkIdentity {Value = value};
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Value.GetHashCode();
        }


        //Helper functions
        public World GetWorld(Universe universe)
        {
            return universe[Value.WorldId];
        }

        public bool TryGetWorld(Universe universe, out World record)
        {
            return universe.TryGetValue(Value.WorldId, out record);
        }


        public bool TryGetChunkEntity(Universe universe, out Entity record)
        {
            if (universe.TryGetValue(Value.WorldId, out var universeRecord))
                return TryGetChunkEntity(universeRecord, out record);

            record = default;
            return false;
        }

        public Entity GetChunkEntity(Universe universe)
        {
            return GetChunkEntity(GetWorld(universe));
        }


        public bool TryGetChunkEntity(World chunkMap, out Entity record)
        {
            return chunkMap.TryGetValue(Value.ChunkId, out record);
        }

        public Entity GetChunkEntity(World chunkMap)
        {
            return chunkMap[Value.ChunkId];
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}