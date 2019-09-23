using System;
using System.Collections.Generic;

namespace Types
{
    public static class DictionaryX
    {
        public static void Dispose<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TValue : IDisposable
        {
            foreach (var kvp in dictionary) kvp.Value.Dispose();
        }
    }
}