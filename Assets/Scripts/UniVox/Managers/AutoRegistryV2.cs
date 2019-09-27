using System;
using System.Collections;
using System.Collections.Generic;

namespace UniVox.Managers
{

    public struct AutoReference<TKey, TValue> : IAutoReference<TKey, TValue>
    {
        public AutoReference(TKey key, int id, TValue value)
        {
            Key = key;
            Id = id;
            Value = value;
        }

        public static AutoReference<TKey, TValue> Create(IDictionary<TKey, int> lookup, IList<TValue> values, int id)
        {
            var key = default(TKey);
            var value = values[id];

            foreach (var kvp in lookup)
                if (kvp.Value == id)
                {
                    key = kvp.Key;
                    break;
                }

            return new AutoReference<TKey, TValue>(key, id, value);
        }

        public static AutoReference<TKey, TValue> Create(IDictionary<TKey, int> lookup, IList<TValue> values, TKey key)
        {
            var id = lookup[key];
            var value = values[id];
            return new AutoReference<TKey, TValue>(key, id, value);
        }

        public TKey Key { get; }
        public int Id { get; }

        public TValue Value { get; }
    }

    /// <summary>
    ///     Wrapper around a dictionary, automatically assigns an integer id to each item in the registry
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TReference"></typeparam>
    public class AutoRegistryV2<TKey, TValue>
        //: IReadOnlyList<TValue>, IReadOnlyDictionary<TKey, TValue>, IIndexedRegistry<TKey,TValue>, IRegistry<TKey, TValue>
        : IAutoRegistryV2<TKey, TValue>
    {
        private readonly List<TValue> _backingArray;
        private readonly Dictionary<TKey, int> _backingLookup;
        private int _nextId;


        public AutoRegistryV2(int initialSize = 0)
        {
            _backingLookup = new Dictionary<TKey, int>(initialSize);
            _backingArray = new List<TValue>(initialSize);
            _nextId = 0;
        }

        public AutoRegistryV2(AutoRegistryV2<TKey, TValue> registry)
        {
            _backingLookup = new Dictionary<TKey, int>(registry._backingLookup, registry._backingLookup.Comparer);
            _backingArray = new List<TValue>(registry._backingArray);
        }


        public int Count => _backingArray.Count;

        public IAutoReference<TKey, TValue> this[TKey key] =>
            AutoReference<TKey, TValue>.Create(_backingLookup, _backingArray, key);

        public IAutoReference<TKey, TValue> this[int index] =>
            AutoReference<TKey, TValue>.Create(_backingLookup, _backingArray, index);

        public bool IsRegistered(TKey key)
        {
            return _backingLookup.ContainsKey(key);
        }

        public bool IsRegistered(int index)
        {
            return (index >= 0 && index < _backingArray.Count);
        }


        private int GetNextId()
        {
            var temp = _nextId;
            //TODO add in a bit that allows us to fetch empty holes in the registry
            _nextId++;
            return temp;
        }

        /// <summary>
        ///     Registers the value into the registry
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool Register(TKey key, TValue value)
        {
            if (IsRegistered(key))
            {
                return false;
            }

            var id = GetNextId();
            _backingLookup[key] = id;
            if (id == _backingArray.Count)
                _backingArray.Add(value);
            else
                _backingArray[id] = value;
            return true;
        }

        public bool Register(TKey key, TValue value, out IAutoReference<TKey, TValue> reference)
        {
            if (Register(key, value))
            {
                reference = this[key];
                return true;
            }
            else
            {
                reference = default;
                return false;
            }
        }

        public bool TryGetReference(TKey key, out IAutoReference<TKey, TValue> value)
        {
            if (IsRegistered(key))
            {
                value = this[key];
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetReference(int index, out IAutoReference<TKey, TValue> value)
        {
            if (IsRegistered(index))
            {
                value = this[index];
                return true;
            }

            value = default;
            return false;
        }
    }
    
}