using System;
using System.Collections.Generic;
using VoxelWorld = UniVox.Core.Types.World.World;

namespace UniVox.Core.Types.Universe
{
    public class Universe : IDisposable, IAccessorMap<byte, World.World>
    {
        private Dictionary<byte, VoxelWorld> _records;

        public Universe()
        {
            _records = new Dictionary<byte, VoxelWorld>();
        }

        public bool ContainsKey(byte key)
        {
            return _records.ContainsKey(key);
        }


        public World.World GetAccessor(byte index)
        {
            throw new NotImplementedException();
        }

        public bool TryGetAccessor(byte key, out VoxelWorld accessor) => TryGetValue(key, out accessor);

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

        public bool TryGetValue(byte worldId, out VoxelWorld record)
        {
            return _records.TryGetValue(worldId, out record);
        }

        public VoxelWorld GetOrCreate(byte worldId, string name = default)
        {
            if (!TryGetValue(worldId, out var world)) _records[worldId] = world = new World.World();

            return world;
        }
    }
}