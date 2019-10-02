using System;
using System.Collections.Generic;

namespace UniVox.Utility
{
    public static class DictionaryX
    {
        public static void DisposeEnumerable<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TValue : IDisposable
        {
            foreach (var kvp in dictionary) kvp.Value.Dispose();
        }
        public static void DisposeEnumerable<TValue>(this IEnumerable<TValue> values) where TValue : IDisposable
        {
            foreach (var value in values) value.Dispose();
        }
    }
}