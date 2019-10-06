using System;
using System.Collections;
using System.Collections.Generic;
using VoxelWorld = UniVox.VoxelData.World;

namespace UniVox.VoxelData
{
    public class Universe : IDisposable, IReadOnlyDictionary<byte, World>
    {
        private readonly Dictionary<byte, VoxelWorld> _records;

        public Universe()
        {
            _records = new Dictionary<byte, VoxelWorld>();
        }

        public void Dispose()
        {
            foreach (var record in _records.Values) record.Dispose();
        }

        public bool ContainsKey(byte key)
        {
            return _records.ContainsKey(key);
        }


        public VoxelWorld this[byte worldId] => _records[worldId];
        public IEnumerable<byte> Keys => ((IReadOnlyDictionary<byte, VoxelWorld>) _records).Keys;

        public IEnumerable<World> Values => ((IReadOnlyDictionary<byte, VoxelWorld>) _records).Values;

        public bool TryFindWorld(Unity.Entities.World world, out VoxelWorld record)
        {
            foreach (var value in _records.Values)
            {
                if (value.EntityWorld == world)
                {
                    record=value;
                    return true;
                }
            }

            record = default;
            return false;
        }
        public bool TryFindWorld(Unity.Entities.World world, out byte id, out VoxelWorld record)
        {
            foreach (var pair in _records)
            {
                if (pair.Value.EntityWorld == world)
                {
                    id = pair.Key;
                    record=pair.Value;
                    return true;
                }
            }

            id = default;
            record = default;
            return false;
        }
        
        public bool TryGetValue(byte worldId, out VoxelWorld record)
        {
            return _records.TryGetValue(worldId, out record);
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

        
        public VoxelWorld GetOrCreate(byte worldId, string name = default)
        {
            if (!TryGetValue(worldId, out var world)) _records[worldId] = world = new VoxelWorld();

            return world;
        }
        public VoxelWorld GetOrCreate(Unity.Entities.World world, out byte id, string name = default)
        {
            if (!TryFindWorld(world, out id, out var record))
            {
                //TODO use an unused ID instead of 0
                id = 0;
                record = GetOrCreate(0, name);
            }

            return record;
        }
    }
}