using System;
using System.Collections.Generic;

namespace Types
{
    [Obsolete("Use Extension Method")]
    public class DisposableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable where TValue : IDisposable
    {
        public void Dispose()
        {
            foreach (var kvp in this) kvp.Value.Dispose();
        }
    }

    public static class DictionaryX
    {
        public static void Dispose<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TValue : IDisposable
        {
            foreach (var kvp in dictionary) kvp.Value.Dispose();
        }
    }
}