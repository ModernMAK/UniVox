using System.Collections;
using System.Collections.Generic;

namespace UniVox.Managers.Generic
{
    /// <summary>
    ///     Wrapper around a dictionary
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class Registry<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IRegistry<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _backingLookup;

        public Registry()
        {
            _backingLookup = new Dictionary<TKey, TValue>();
        }

        public Registry(Registry<TKey, TValue> registry)
        {
            _backingLookup = new Dictionary<TKey, TValue>(registry._backingLookup, registry._backingLookup.Comparer);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _backingLookup.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public int Count => _backingLookup.Count;

        public bool ContainsKey(TKey key)
        {
            return _backingLookup.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _backingLookup.TryGetValue(key, out value);
        }

        public TValue this[TKey key] => _backingLookup[key];

        public IEnumerable<TKey> Keys => _backingLookup.Keys;

        public IEnumerable<TValue> Values => _backingLookup.Values;

        public bool IsRegistered(TKey key)
        {
            return ContainsKey(key);
        }

        /// <summary>
        ///     Registers the value into the registry, overwrites previous registrations if present.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool Register(TKey key, TValue value)
        {
            if (_backingLookup.ContainsKey(key)) return false;
            _backingLookup[key] = value;
            return true;
        }

        public virtual bool Unregister(TKey key)
        {
            return _backingLookup.Remove(key);
        }
    }
}