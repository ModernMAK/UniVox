using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace UniVox.Core
{
    public partial class ChunkMap : IDisposable
    {
//        private EntityWorld _entityWorld;
        private readonly Dictionary<int3, Data> _records;

        public ChunkMap() //string name = default)
        {
            //For now, I assume we will never have more than 255 chunks
            //I know for a fact we probably will, but I digress
            _records = new Dictionary<int3, Data>(byte.MaxValue);
        }

        public Data this[int3 index] => _records[index];


        public void Dispose()
        {
            foreach (var recordValue in _records.Values)
            {
                recordValue.Dispose();
            }
        }

        public bool TryGetValue(int3 valueChunkId, out Data value) => _records.TryGetValue(valueChunkId, out value);
    }

    public class World
    {
        public World()
        {
            _chunks = new ChunkMap();
            _entityWorldMap = new EntityWorldMap();
        }
        
        private ChunkMap _chunks;
        private EntityWorldMap _entityWorldMap;

        public ChunkMap Chunks => _chunks;
        public EntityWorldMap Entities => _entityWorldMap;
        
        public struc

    }
}