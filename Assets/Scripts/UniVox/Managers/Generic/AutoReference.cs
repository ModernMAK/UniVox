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
}