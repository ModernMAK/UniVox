using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Types
{
    public struct VoxelShapes : ISharedComponentData, IEquatable<VoxelShapes>, IDictionary<BlockShape, Mesh>
    {
        public IDictionary<BlockShape, Mesh> Lookup;

        public bool Equals(VoxelShapes other)
        {
            return Equals(Lookup, other.Lookup);
        }

        public IEnumerator<KeyValuePair<BlockShape, Mesh>> GetEnumerator()
        {
            return Lookup.GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelShapes other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Lookup != null ? Lookup.GetHashCode() : 0;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Lookup.GetEnumerator();
        }

        public void Add(KeyValuePair<BlockShape, Mesh> item)
        {
            Lookup.Add(item);
        }

        public void Clear()
        {
            Lookup.Clear();
        }

        public bool Contains(KeyValuePair<BlockShape, Mesh> item)
        {
            return Lookup.Contains(item);
        }

        public void CopyTo(KeyValuePair<BlockShape, Mesh>[] array, int arrayIndex)
        {
            Lookup.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<BlockShape, Mesh> item)
        {
            return Lookup.Remove(item);
        }

        public int Count => Lookup.Count;

        public bool IsReadOnly => Lookup.IsReadOnly;

        public void Add(BlockShape key, Mesh value)
        {
            Lookup.Add(key, value);
        }

        public bool ContainsKey(BlockShape key)
        {
            return Lookup.ContainsKey(key);
        }

        public bool Remove(BlockShape key)
        {
            return Lookup.Remove(key);
        }

        public bool TryGetValue(BlockShape key, out Mesh value)
        {
            return Lookup.TryGetValue(key, out value);
        }

        public Mesh this[BlockShape key]
        {
            get => Lookup[key];
            set => Lookup[key] = value;
        }

        public ICollection<BlockShape> Keys => Lookup.Keys;

        public ICollection<Mesh> Values => Lookup.Values;
    }
}