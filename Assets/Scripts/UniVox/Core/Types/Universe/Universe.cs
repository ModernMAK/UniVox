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

        public void Dispose()
        {
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