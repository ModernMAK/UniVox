using System;
using Unity.Entities;

namespace UniVox.Core
{
    public struct ChunkIdComponent : IComponentData, IEquatable<ChunkIdComponent>, IComparable<ChunkIdComponent>
    {
        /*
         * A not beause i keep thinking i need to do this. BECAUSE entities are stored on a per World Basis, we dont need to use a Shared Component To GRoup Them
         * We do still need the WorldID since we dont store a reference to the World
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


        //Helper functions
        public World GetWorld(Universe universe) => universe[Value.WorldId];

        public bool TryGetWorld(Universe universe, out World record) =>
            universe.TryGetValue(Value.WorldId, out record);


        public bool TryGetChunk(Universe universe, out World.Record record)
        {
            if (universe.TryGetValue(Value.WorldId, out var universeRecord))
            {
                return TryGetChunkRecord(universeRecord, out record);
            }

            record = default;
            return false;
        }

        public World.Record GetChunk(Universe universe) => GetChunk(GetWorld(universe));


        public bool TryGetChunkRecord(World chunkMap, out World.Record record) =>
            chunkMap.TryGetAccessor(Value.ChunkId, out record);

        public World.Record GetChunk(World chunkMap) => chunkMap[Value.ChunkId];
    }
}