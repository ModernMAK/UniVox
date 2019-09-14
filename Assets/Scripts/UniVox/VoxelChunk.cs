using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;



namespace Univox
{
    [Obsolete]
    public class VoxelChunk : IReadOnlyDictionary<int3, Entity>, IDisposable
    {
        public const int AxisSize = 8;
        public const int SquareSize = AxisSize * AxisSize;
        public const int CubeSize = SquareSize * AxisSize;
        private NativeArray<Entity> _backing;


        /// <summary>
        /// This is a handle used to modify native array this chunk uses, it should only be used to access the contents without relying on this class, OR when a NativeArray is needed instead of a list 
        /// </summary>
        /// <returns>The native array used by the chunk.</returns>
        public NativeArray<Entity> GetNativeHandle() => _backing;

        public AxisOrdering PositionOrder { get; set; }


        private static readonly IDictionary<AxisOrdering, IReadOnlyList<int3>> ChunkOrderedKeys = GenerateKeyTable();
        private static IReadOnlyList<int3> GetChunkKeys(AxisOrdering order) => ChunkOrderedKeys[order];

        private static IDictionary<AxisOrdering, IReadOnlyList<int3>> GenerateKeyTable()
        {
            var keyTable = new Dictionary<AxisOrdering, IReadOnlyList<int3>>();
            foreach (var order in AxisOrderingX.Values)
            {
                keyTable[order] = GenerateKeys(order);
            }

            return keyTable;
        }

        private static IReadOnlyList<int3> GenerateKeys(AxisOrdering order)
        {
            var keys = new int3[CubeSize];
            for (var x = 0; x < AxisSize; x++)
            for (var y = 0; y < AxisSize; y++)
            for (var z = 0; z < AxisSize; z++)
            {
                keys[PositionUtil.GetIndex(x, y, z, order)] = new int3(x, y, z);
            }

            return keys;
        }

        public IEnumerator<KeyValuePair<int3, Entity>> GetEnumerator()
        {
            var keys = GetChunkKeys(PositionOrder);
            var values = _backing;
            for (var i = 0; i < Count; i++)
            {
                yield return new KeyValuePair<int3, Entity>(keys[i], values[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => CubeSize;

        public bool ContainsKey(int3 key)
        {
            return (key.x >= 0 && key.x < AxisSize) && (key.y >= 0 && key.y < AxisSize) &&
                   (key.z >= 0 && key.z < AxisSize);
        }

        public bool TryGetValue(int3 key, out Entity value)
        {
            if (ContainsKey(key))
            {
                value = this[key];
                return true;
            }

            value = default;
            return false;
        }

        public Entity this[int3 key]
        {
            get => _backing[PositionUtil.GetIndex(key, PositionOrder)];
            set => _backing[PositionUtil.GetIndex(key, PositionOrder)] = value;
        }

        public IEnumerable<int3> Keys => GetChunkKeys(PositionOrder);
        public IEnumerable<Entity> Values => _backing;

        public void Dispose()
        {
            _backing.Dispose();
        }
    }
}