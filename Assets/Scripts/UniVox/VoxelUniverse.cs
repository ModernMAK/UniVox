using System.Collections;
using System.Collections.Generic;

namespace Univox
{
    public class VoxelUniverse : IReadOnlyDictionary<byte, VoxelWorld>
    {
        private readonly Dictionary<byte, VoxelWorld> _backing;

        public VoxelUniverse()
        {
            _backing = new Dictionary<byte, VoxelWorld>();
        }

        public IEnumerator<KeyValuePair<byte, VoxelWorld>> GetEnumerator()
        {
            return _backing.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _backing).GetEnumerator();
        }

        public VoxelWorld CreateWorld(string name = default)
        {
            return _backing[(byte)_backing.Count] = new VoxelWorld(name);
        }

        public int Count => _backing.Count;

        public bool ContainsKey(byte key)
        {
            return _backing.ContainsKey(key);
        }

        public bool TryGetValue(byte key, out VoxelWorld value)
        {
            return _backing.TryGetValue(key, out value);
        }

        public VoxelWorld this[byte key]
        {
            get => _backing[key];
            set => _backing[key] = value;
        }

        public IEnumerable<byte> Keys => _backing.Keys;

        public IEnumerable<VoxelWorld> Values => _backing.Values;
    }
}