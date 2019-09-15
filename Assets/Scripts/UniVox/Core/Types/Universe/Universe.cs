using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace UniVox.Core
{
    public partial class Universe : IDisposable, IAccessorMap<byte,World>
    {
        private Dictionary<byte, World> _records;
//
//        public class Record : IDisposable
//        {
//            public Record(string name = default)
//            {
//                _chunkMap = new ChunkMap();
//                _entityWorldMap = new EntityWorldMap(name);
//            }
//
//
//            private readonly ChunkMap _chunkMap;
//
//            private readonly EntityWorldMap _entityWorldMap;
//
////            private EntityWorld _entityWorld;
//            public ChunkMap ChunkMap => _chunkMap;
//            public EntityWorldMap EntityWorldMap => _entityWorldMap;
//            
//            
//            public Record this[int3 worldId] => _records[worldId];
//
//            public bool TryGetValue(byte worldId, out Record record) => _records.TryGetValue(worldId, out record);
//
//            public Record GetOrCreate(byte worldId, string name = default)
//            {
//                return new Record(name);
//            }
//
//            public void Dispose()
//            {
//                _chunkMap?.Dispose();
//                _entityWorldMap?.Dispose();
//            }
//        }

        public void Dispose()
        {
//            foreach (var record in _world.)
//            {
//                
//            }
            foreach (var record in _records.Values)
            {
                record.Dispose();
            }
        }

        public bool ContainsKey(byte key)
        {
            return _records.ContainsKey(key);
        }


        public World GetAccessor(byte index)
        {
            throw new NotImplementedException();
        }

        public bool TryGetAccessor(byte key, out World accessor)
        {
            throw new NotImplementedException();
        }

        public World this[byte worldId] => _records[worldId];

        public bool TryGetValue(byte worldId, out World record) => _records.TryGetValue(worldId, out record);

        public World GetOrCreate(byte worldId, string name = default)
        {
            if (!TryGetAccessor(worldId, out var world))
            {
                _records[worldId] = world = new World();
            }

            return world;
        }
    }
}