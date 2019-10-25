using System;
using System.Collections.Generic;
using UnityEngine.Rendering.LookDev;

namespace UniVox.Managers.Game.Accessor
{
    //Whats nice about registries, is that they want to be 3-way dictionaries, but they are readonly.
    //Instead of 6 dictionaries, we only need 3; one per Key,Id,Value, and then an array for the pair
    //In our case, ID double as the index, so we only need 2

    [Obsolete]
    public abstract class RegistryWrapper<TKey, TIdentity, TValue>
    {
        public TValue this[TKey key] => GetValue(key);
        public TValue this[TIdentity identity] => GetValue(identity);

        public bool Register(TKey key, TValue value)
        {
            return Register(key, value, out _);
        }

        public abstract bool Register(TKey key, TValue value, out TIdentity identity);


        public abstract IEnumerable<Pair> GetAllRegistered();

        public abstract bool IsRegistered(TKey key);
        public abstract bool IsRegistered(TIdentity identity);

        public abstract TIdentity GetIdentity(TKey key);
        public abstract bool TryGetIdentity(TKey key, out TIdentity identity);

        public abstract TValue GetValue(TKey key);
        public abstract bool TryGetValue(TKey key, out TValue value);

        public abstract TValue GetValue(TIdentity identity);
        public abstract bool TryGetValue(TIdentity identity, out TValue value);

        public struct Pair
        {
            public TKey Key;
            public TIdentity Identity;
            public TValue Value;
        }
    }
}