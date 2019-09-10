using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;

namespace InventorySystem
{
    public interface IRegistry<in TKey, TValue>
    {
        bool Register(TKey key, TValue value);
        bool Unregister(TKey key);

        TValue this[TKey key] { get; }

        bool TryGetValue(TKey key, out TValue value);
        bool IsRegistered(TKey key);
    }

    /// <summary>
    /// Utility registry lookup via string
    /// </summary>
    public class NamedRegistry<TValue> : AutoRegistry<string, TValue>
    {
    }
    
    /// <summary>
    /// Utility registry lookup via integer
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class IndexedRegistry<TValue> : Registry<int, TValue>
    {
    }

    /// <summary>
    /// Wrapper around a dictionary, automatically assigns an integer id to each item in the registry
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class AutoRegistry<TKey, TValue> : IReadOnlyList<TValue>, IReadOnlyDictionary<TKey, TValue>,
        IRegistry<TKey, TValue>
    {
        private readonly Dictionary<TKey, int> _backingLookup;
        private readonly List<TValue> _backingArray;
        private int _nextId;


        public bool IsRegistered(TKey key) => ContainsKey(key);
        public AutoRegistry(int initialSize = 0)
        {
            _backingLookup = new Dictionary<TKey, int>(initialSize);
            _backingArray = new List<TValue>(initialSize);
            _nextId = 0;
        }

        public AutoRegistry(AutoRegistry<TKey, TValue> registry)
        {
            _backingLookup = new Dictionary<TKey, int>(registry._backingLookup, registry._backingLookup.Comparer);
            _backingArray = new List<TValue>(registry._backingArray);
        }

        private int GetNextId()
        {
            var temp = _nextId;
            //TODO add in a bit that allows us to fetch empty holes in the registry
            _nextId++;
            return temp;
        }

        /// <summary>
        /// Registers the value into the registry, overwrites previous registrations if present.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool Register(TKey key, TValue value)
        {
            if (ContainsKey(key)) return false;

            var nId = GetNextId();
            _backingLookup[key] = nId;
            _backingArray[nId] = value;
            return true;
        }

        public bool Unregister(TKey key)
        {
            //If it doesnt exist, dont unregister anything
            if (!TryGetIndex(key, out var index))
                return false;

            _backingArray[index] = default;
            _backingLookup.Remove(key);
            return true;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            foreach (var kvp in _backingLookup)
            {
                yield return new KeyValuePair<TKey, TValue>(kvp.Key, _backingArray[kvp.Value]);
            }
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return _backingArray.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>) this).GetEnumerator();
        }

        int IReadOnlyCollection<TValue>.Count => _backingArray.Count;

        public TValue this[int index]
        {
            get => _backingArray[index];
            set => _backingArray[index] = value;
        }

        int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => _backingLookup.Count;

        public bool ContainsKey(TKey key)
        {
            return _backingLookup.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (TryGetIndex(key, out var index))
            {
                value = _backingArray[index];
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetIndex(TKey key, out int value)
        {
            return _backingLookup.TryGetValue(key, out value);
        }

        public TValue this[TKey key] => _backingArray[_backingLookup[key]];


        public IEnumerable<TKey> Keys => _backingLookup.Keys;

        public IEnumerable<TValue> Values => _backingArray;
    }

    /// <summary>
    /// Wrapper around a dictionary
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class Registry<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IRegistry<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _backingLookup;

        public bool IsRegistered(TKey key) => ContainsKey(key);
        
        public Registry()
        {
            _backingLookup = new Dictionary<TKey, TValue>();
        }

        public Registry(Registry<TKey, TValue> registry)
        {
            _backingLookup = new Dictionary<TKey, TValue>(registry._backingLookup, registry._backingLookup.Comparer);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _backingLookup.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public int Count => _backingLookup.Count;

        public bool ContainsKey(TKey key) => _backingLookup.ContainsKey(key);

        public bool TryGetValue(TKey key, out TValue value) => _backingLookup.TryGetValue(key, out value);

        public TValue this[TKey key] => _backingLookup[key];

        public IEnumerable<TKey> Keys => _backingLookup.Keys;

        public IEnumerable<TValue> Values => _backingLookup.Values;

        /// <summary>
        /// Registers the value into the registry, overwrites previous registrations if present.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool Register(TKey key, TValue value)
        {
            if (_backingLookup.ContainsKey(key)) return false;
            _backingLookup[key] = value;
            return true;
        }

        public virtual bool Unregister(TKey key) => _backingLookup.Remove(key);
    }
}