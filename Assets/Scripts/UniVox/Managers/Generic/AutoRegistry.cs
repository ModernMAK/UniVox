using System.Collections;
using System.Collections.Generic;

namespace UniVox.Managers.Generic
{
    public struct NameIndexValue<TKey, TValue>
    {
        public TKey Key;
        public int Index;
        public TValue Value;
    }

    /// <summary>
    ///     Wrapper around a dictionary, automatically assigns an integer identity to each item in the registry
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class AutoRegistry<TKey, TValue> : IReadOnlyList<TValue>, IReadOnlyDictionary<TKey, TValue>,
        IIndexedRegistry<TKey, TValue>,
        IRegistry<TKey, TValue> //, IEnumerable<KeyValuePair<int, TValue>>
    {
        private readonly List<TValue> _backingArray;
        private readonly Dictionary<TKey, int> _backingLookup;
        private int _nextId;


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

        public bool Register(TKey key, TValue value, out int id)
        {
            if (ContainsKey(key))
            {
                id = default;
                return false;
            }

            id = GetNextId();
            _backingLookup[key] = id;
            if (id == _backingArray.Count)
                _backingArray.Add(value);
            else
                _backingArray[id] = value;
            return true;
        }

        public bool TryGetValue(int key, out TValue value)
        {
            if (_backingArray.Count > key && key >= 0)
            {
                value = _backingArray[key];
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetIndex(TKey key, out int value)
        {
            return _backingLookup.TryGetValue(key, out value);
        }

        public bool IsRegistered(int index)
        {
            return index >= 0 && index < _backingArray.Count;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            foreach (var value in GetKeyValuePairs()) yield return value;
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

        public TValue this[TKey key] => _backingArray[_backingLookup[key]];


        public IEnumerable<TKey> Keys => _backingLookup.Keys;

        public IEnumerable<TValue> Values => _backingArray;

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


        public bool IsRegistered(TKey key)
        {
            return ContainsKey(key);
        }

        /// <summary>
        ///     Registers the value into the registry
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool Register(TKey key, TValue value)
        {
            return Register(key, value, out _);
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


        public IEnumerable<KeyValuePair<TKey, TValue>> GetKeyValuePairs()
        {
            foreach (var kvp in _backingLookup)
                yield return new KeyValuePair<TKey, TValue>(kvp.Key, _backingArray[kvp.Value]);
        }

//        IEnumerator<KeyValuePair<int, TValue>> IEnumerable<KeyValuePair<int, TValue>>.GetEnumerator()
//        {
//            foreach (var value in GetIndexedValuePairs())
//            {
//                yield return value;
//            }
//        }

        public IEnumerable<KeyValuePair<int, TValue>> GetIndexedValuePairs()
        {
            for (var i = 0; i < _backingArray.Count; i++)
                yield return new KeyValuePair<int, TValue>(i, _backingArray[i]);
        }

        public IEnumerable<NameIndexValue<TKey, TValue>> GetNameIndexValuePairs()
        {
            foreach (var kvp in _backingLookup)
                yield return new NameIndexValue<TKey, TValue>
                {
                    Key = kvp.Key,
                    Index = kvp.Value,
                    Value = _backingArray[kvp.Value]
                };
        }

        private int GetNextId()
        {
            var temp = _nextId;
            //TODO add in a bit that allows us to fetch empty holes in the registry
            _nextId++;
            return temp;
        }
    }
}