using System.Collections;
using System.Collections.Generic;

namespace InventorySystem
{
    public class Registry<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        protected readonly Dictionary<TKey, TValue> _dictionary;

        public Registry()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        public Registry(Registry<TKey, TValue> registry)
        {
            _dictionary = new Dictionary<TKey, TValue>(registry._dictionary, registry._dictionary.Comparer);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public int Count => _dictionary.Count;

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

        public TValue this[TKey key] => _dictionary[key];

        public IEnumerable<TKey> Keys => _dictionary.Keys;

        public IEnumerable<TValue> Values => _dictionary.Values;

        /// <summary>
        /// Registers the value into the registry, overwrites previous registrations if present.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public virtual void Register(TKey key, TValue value)
        {
            _dictionary[key] = value;
        }

        public virtual bool Unregister(TKey key) => _dictionary.Remove(key);
    }
}