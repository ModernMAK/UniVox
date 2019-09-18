using System;
using System.Collections.Generic;

namespace UniVox.Core.Types.Universe
{
    public class Universe : IDisposable, IAccessorMap<byte, World.World>
    {
        private Dictionary<byte, World.World> _records;

        public bool ContainsKey(byte key)
        {
            return _records.ContainsKey(key);
        }


        public World.World GetAccessor(byte index)
        {
            throw new NotImplementedException();
        }

        public bool TryGetAccessor(byte key, out World.World accessor)
        {
            throw new NotImplementedException();
        }

        public World.World this[byte worldId] => _records[worldId];
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
            foreach (var record in _records.Values) record.Dispose();
        }

        public bool TryGetValue(byte worldId, out World.World record)
        {
            return _records.TryGetValue(worldId, out record);
        }

        public World.World GetOrCreate(byte worldId, string name = default)
        {
            if (!TryGetAccessor(worldId, out var world)) _records[worldId] = world = new World.World();

            return world;
        }
    }
}