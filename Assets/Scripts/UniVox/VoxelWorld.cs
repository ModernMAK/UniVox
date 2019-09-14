using System;
using System.Collections;
using System.Collections.Generic;
using InventorySystem;
using Unity.Entities;
using Unity.Mathematics;


using World = Unity.Entities.World;
namespace Univox
{
    
    
    
    [Obsolete]
    public class VoxelWorld : IReadOnlyDictionary<int3, VoxelChunk>
    {
        private readonly Dictionary<int3, VoxelChunk> _backing;
        private readonly Unity.Entities.World _entityWorld;

        public VoxelWorld(string worldName = default)
        {
            _backing = new Dictionary<int3, VoxelChunk>();
            _entityWorld = new Unity.Entities.World(worldName);
        }

        public VoxelChunk CreateChunk(int3 chunkPosition)
        {
            return _backing[chunkPosition] = new VoxelChunk();
        }

        public bool TryCreateChunk(int3 chunkPosition, out VoxelChunk chunk)
        {
            if (!ContainsKey(chunkPosition))
            {
                chunk = CreateChunk(chunkPosition);
                return true;
            }

            chunk = default;
            return false;
        }

        public Unity.Entities.World UnityWorld => _entityWorld;

        public IEnumerator<KeyValuePair<int3, VoxelChunk>> GetEnumerator()
        {
            return _backing.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _backing.Count;
        public bool ContainsKey(int3 key) => _backing.ContainsKey(key);
        public bool TryGetValue(int3 key, out VoxelChunk value) => _backing.TryGetValue(key, out value);

        public VoxelChunk this[int3 key]
        {
            get => _backing[key];
            set => _backing[key] = value;
        }

        public IEnumerable<int3> Keys => _backing.Keys;
        public IEnumerable<VoxelChunk> Values => _backing.Values;
    }
}