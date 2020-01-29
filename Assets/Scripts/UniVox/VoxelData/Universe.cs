using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;

namespace UniVox
{
    [Obsolete]
    public class Universe : IDisposable, IReadOnlyDictionary<byte, WorldMap>
    {
        private readonly Dictionary<byte, WorldMap> _records;

        public Universe()
        {
            _records = new Dictionary<byte, WorldMap>();
        }

        public void Dispose()
        {
            foreach (var record in _records.Values) record.Dispose();
        }

        public bool ContainsKey(byte key)
        {
            return _records.ContainsKey(key);
        }


        public WorldMap this[byte worldId] => _records[worldId];
        public IEnumerable<byte> Keys => ((IReadOnlyDictionary<byte, WorldMap>) _records).Keys;

        public IEnumerable<WorldMap> Values => ((IReadOnlyDictionary<byte, WorldMap>) _records).Values;

        public bool TryGetValue(byte worldId, out WorldMap record)
        {
            return _records.TryGetValue(worldId, out record);
        }

        public IEnumerator<KeyValuePair<byte, WorldMap>> GetEnumerator()
        {
            return _records.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _records).GetEnumerator();
        }

        public int Count => _records.Count;

        public bool TryFindWorld(World world, out WorldMap record)
        {
            foreach (var value in _records.Values)
                if (value.EntityWorld == world)
                {
                    record = value;
                    return true;
                }

            record = default;
            return false;
        }

        public bool TryFindWorld(World world, out byte id, out WorldMap record)
        {
            foreach (var pair in _records)
                if (pair.Value.EntityWorld == world)
                {
                    id = pair.Key;
                    record = pair.Value;
                    return true;
                }

            id = default;
            record = default;
            return false;
        }


        public WorldMap GetOrCreate(byte worldId, string name = default)
        {
            if (!TryGetValue(worldId, out var world)) _records[worldId] = world = new WorldMap();

            return world;
        }

        public WorldMap GetOrCreate(World world, out byte id, string name = default)
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