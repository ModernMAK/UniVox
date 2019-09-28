using System;
using System.Collections;
using System.Collections.Generic;
using VoxelWorld = UniVox.VoxelData.World;

namespace UniVox.VoxelData
{
    public class Universe : IDisposable, IReadOnlyDictionary<byte, World>
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


        public VoxelWorld this[byte worldId] => _records[worldId];
        public IEnumerable<byte> Keys => ((IReadOnlyDictionary<byte, VoxelWorld>) _records).Keys;

        public IEnumerable<World> Values => ((IReadOnlyDictionary<byte, VoxelWorld>) _records).Values;

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
            if (!TryGetValue(worldId, out var world)) _records[worldId] = world = new VoxelWorld();

            return world;
        }

        public IEnumerator<KeyValuePair<byte, World>> GetEnumerator()
        {
            return _records.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _records).GetEnumerator();
        }

        public int Count => _records.Count;
    }
}