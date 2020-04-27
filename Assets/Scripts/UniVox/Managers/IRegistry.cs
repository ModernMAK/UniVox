using System.Collections.Generic;

namespace UniVox.Managers
{
    public interface IRegistry<in TKey, TIdentity, TValue> : IReadOnlyCollection< TValue>
    {
        int Register(TKey key, TValue value);
        bool TryRegister(TKey key, TValue value, out TIdentity identity);
        
        bool IsRegistered(TKey key);
        bool IsRegistered(TIdentity identity);
        
        bool TryGetValue(TIdentity identity, out TValue value);
        bool TryGetValue(TKey key, out TValue value);
        
        TValue GetValue(TIdentity identity);
        TValue GetValue(TKey key);
        
        bool TryGetIdentity(TKey key, out TIdentity identity);
        int GetIdentity(TKey key);
        
        TValue this[TKey key] { get; }
        TValue this[TIdentity identity] { get; }
    }
}