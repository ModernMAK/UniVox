using System;
using System.Collections;
using System.Collections.Generic;

namespace UniVox.Managers.Game.Accessor
{
    public abstract class BaseRegistry<TKey, TIdentity, TValue> : IReadOnlyCollection<TValue>
    {
        private class MapWrapperDictionary<TDKey, TDValue> : IReadOnlyDictionary<TDKey, TDValue>
        {
            private readonly IReadOnlyDictionary<TDKey, int> _indexMap;
            private readonly IReadOnlyList<Record> _records;
            private readonly Func<Record, TDValue> _unwrap;

            public MapWrapperDictionary(IReadOnlyDictionary<TDKey, int> indexMap, IReadOnlyList<Record> records,
                Func<Record, TDValue> unwrap)
            {
                _indexMap = indexMap;
                _records = records;
                _unwrap = unwrap;
            }


            public IEnumerator<KeyValuePair<TDKey, TDValue>> GetEnumerator()
            {
                foreach (var pair in _indexMap)
                {
                    var key = pair.Key;
                    var value = _unwrap(_records[pair.Value]);
                    yield return new KeyValuePair<TDKey, TDValue>(key, value);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count => _records.Count;

            public bool ContainsKey(TDKey key) => _indexMap.ContainsKey(key);

            public bool TryGetValue(TDKey key, out TDValue value)
            {
                if (_indexMap.TryGetValue(key, out var index))
                {
                    value = _unwrap(_records[index]);
                    return true;
                }

                value = default;
                return false;
            }

            public TDValue this[TDKey key]
            {
                get { return _unwrap(_records[_indexMap[key]]); }
            }

            public IEnumerable<TDKey> Keys => _indexMap.Keys;

            public IEnumerable<TDValue> Values
            {
                get
                {
                    foreach (var index in _indexMap.Values)
                    {
                        yield return _unwrap(_records[index]);
                    }
                }
            }
        }

        private class IndexWrapperDictionary<TDKey, TDValue> : IReadOnlyDictionary<TDKey, TDValue>
        {
            private readonly IReadOnlyList<Record> _records;
            private readonly Func<Record, TDValue> _unwrap;
            private readonly Func<int, TDKey> _createId;
            private readonly Func<TDKey, int> _getIndex;

            public IndexWrapperDictionary(IReadOnlyList<Record> records, Func<Record, TDValue> unwrap,
                Func<int, TDKey> createId, Func<TDKey, int> getIndex)
            {
                _records = records;
                _unwrap = unwrap;
                _createId = createId;
                _getIndex = getIndex;
            }


            public IEnumerator<KeyValuePair<TDKey, TDValue>> GetEnumerator()
            {
                for (var i = 0; i < _records.Count; i++)
                {
                    var key = _createId(i);
                    var value = _unwrap(_records[i]);
                    yield return new KeyValuePair<TDKey, TDValue>(key, value);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public int Count => _records.Count;

            public bool ContainsKey(TDKey key)
            {
                var index = _getIndex(key);
                return index >= 0 && index < Count;
            }

            public bool TryGetValue(TDKey key, out TDValue value)
            {
                var index = _getIndex(key);
                if (index >= 0 && index < Count)
                {
                    value = _unwrap(_records[index]);
                    return true;
                }

                value = default;
                return false;
            }

            public TDValue this[TDKey key]
            {
                get { return _unwrap(_records[_getIndex(key)]); }
            }

            public IEnumerable<TDKey> Keys
            {
                get
                {
                    for (var i = 0; i < _records.Count; i++)
                    {
                        yield return _createId(i);
                    }
                }
            }

            public IEnumerable<TDValue> Values
            {
                get
                {
                    foreach (var wrapped in _records)
                    {
                        yield return _unwrap(wrapped);
                    }
                }
            }
        }

        private readonly Dictionary<TKey, int> _keys;

//        private Dictionary<TIdentity, int> _identities;
//        private readonly Dictionary<TValue, int> _values;
        private readonly List<Record> _records;

        //4 Accessors, I dont think we ever care about Value to ID or KEY

//        public IReadOnlyDictionary<TKey, TIdentity> KeyToIdentityMap { get; }
//        public IReadOnlyDictionary<TKey, TValue> KeyToValueMap { get; }
//
        public IReadOnlyDictionary<TKey, TValue> KeyToValueMap { get; }
        public IReadOnlyDictionary<TKey, TIdentity> KeyToIdentityMap { get; }
        public IReadOnlyDictionary<TIdentity, TValue> IdentityMap { get; }

        protected BaseRegistry()
        {
            _keys = new Dictionary<TKey, int>();
            _records = new List<Record>();

            KeyToIdentityMap = new MapWrapperDictionary<TKey, TIdentity>(_keys, _records, UnwrapIdentity);
            KeyToValueMap = new MapWrapperDictionary<TKey, TValue>(_keys, _records, UnwrapValue);
            IdentityMap = new IndexWrapperDictionary<TIdentity, TValue>(_records, UnwrapValue, CreateId, GetIndex);
        }

        private static TKey UnwrapKey(Record record) => record.Key;
        private static TIdentity UnwrapIdentity(Record record) => record.Identity;
        private static TValue UnwrapValue(Record record) => record.Value;


        private struct Record
        {
            public Record(TKey key, TIdentity identity, TValue value)
            {
                Key = key;
                Identity = identity;
                Value = value;
            }

            public TKey Key { get; }
            public TIdentity Identity { get; }
            public TValue Value { get; }

            public static implicit operator TKey(Record record) => record.Key;
            public static implicit operator TIdentity(Record record) => record.Identity;
            public static implicit operator TValue(Record record) => record.Value;
        }

        protected abstract TIdentity CreateId(int index);
        protected abstract int GetIndex(TIdentity identity);

        public TIdentity Register(TKey key, TValue value)
        {
            if (!TryRegister(key, value, out var id))
                throw new Exception();
            return id;
        }

        public bool TryRegister(TKey key, TValue value, out TIdentity identity)
        {
            if (_keys.ContainsKey(key)) // || _values.IsRegistered(value))
            {
                identity = default;
                return false;
            }

            var index = _records.Count;
            identity = CreateId(index);
            var record = new Record(key, identity, value);

            _keys[key] = index;
//            _identities[identity] = index;
//            _values[value] = index;
            _records.Add(record);

            return true;
        }


        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var record in _records)
            {
                yield return record.Value;
            }
        }


        public bool IsRegistered(TKey key)
        {
            return _keys.ContainsKey(key);
        }

        public bool IsRegistered(TIdentity key)
        {
            //we could use any IdentityTo*Map, they all SHOULD funciton the same
            return IdentityMap.ContainsKey(key);
        }

        public bool TryGetValue(TIdentity key, out TValue value)
        {
            return IdentityMap.TryGetValue(key, out value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return KeyToValueMap.TryGetValue(key, out value);
        }


        public TValue GetValue(TIdentity key)
        {
            return IdentityMap[key];
        }

        public TValue GetValue(TKey key)
        {
            return KeyToValueMap[key];
        }

        public bool TryGetIdentity(TKey key, out TIdentity identity)
        {
            return KeyToIdentityMap.TryGetValue(key, out identity);
        }

        public TIdentity GetIdentity(TKey key)
        {
            return KeyToIdentityMap[key];
        }

        public TValue this[TKey key] => KeyToValueMap[key];


        public TValue this[TIdentity key] => IdentityMap[key];

        public int Count => _records.Count;
    }
}