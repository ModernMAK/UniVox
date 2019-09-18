using System;
using Unity.Entities;

namespace UniVox.Core.Types
{
    public struct ChunkIdComponent : IComponentData, IEquatable<ChunkIdComponent>, IComparable<ChunkIdComponent>
    {
        /*
         * A note because i keep thinking i need to do this. BECAUSE entities are stored on a per World Basis, we dont need to use a Shared Component To GRoup Them
         * We do still need the WorldID since we dont store a reference to the World (Even though we know the entity knows the EntityWorld their in)
         */
        public UniversalChunkId Value;


        public bool Equals(ChunkIdComponent other)
        {
            return Value.Equals(other.Value);
        }

        public int CompareTo(ChunkIdComponent other)
        {
            return Value.CompareTo(other.Value);
        }

        public static implicit operator UniversalChunkId(ChunkIdComponent component)
        {
            return component.Value;
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Value.GetHashCode();
        }


        //Helper functions
        public World.World GetWorld(Universe.Universe universe)
        {
            return universe[Value.WorldId];
        }

        public bool TryGetWorld(Universe.Universe universe, out World.World record)
        {
            return universe.TryGetValue(Value.WorldId, out record);
        }


        public bool TryGetChunk(Universe.Universe universe, out World.World.Record record)
        {
            if (universe.TryGetValue(Value.WorldId, out var universeRecord))
                return TryGetChunkRecord(universeRecord, out record);

            record = default;
            return false;
        }

        public World.World.Record GetChunk(Universe.Universe universe)
        {
            return GetChunk(GetWorld(universe));
        }


        public bool TryGetChunkRecord(World.World chunkMap, out World.World.Record record)
        {
            return chunkMap.TryGetAccessor(Value.ChunkId, out record);
        }

        public World.World.Record GetChunk(World.World chunkMap)
        {
            return chunkMap[Value.ChunkId];
        }
    }
}