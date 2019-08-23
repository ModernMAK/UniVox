using System;
using System.Collections.Generic;

namespace Types
{
    public class DisposableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable where TValue : IDisposable
    {
        public void Dispose()
        {
            foreach (var kvp in this)
            {
                kvp.Value.Dispose();
            }
        }
    }
}