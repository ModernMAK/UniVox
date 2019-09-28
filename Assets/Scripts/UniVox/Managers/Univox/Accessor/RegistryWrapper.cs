namespace UniVox.Entities.Systems.Registry
{
    public abstract class RegistryWrapper<TKey, TIdentity, TValue>
    {
        public bool Register(TKey key, TValue value) => Register(key, value, out _);
        public abstract bool Register(TKey key, TValue value, out TIdentity identity);


        public TValue this[TKey key] => GetValue(key);
        public TValue this[TIdentity identity] => GetValue(identity);

        public abstract bool IsRegistered(TKey key);
        public abstract bool IsRegistered(TIdentity identity);

        public abstract TIdentity GetIdentity(TKey key);
        public abstract bool TryGetIdentity(TKey key, out TIdentity identity);

        public abstract TValue GetValue(TKey key);
        public abstract bool TryGetValue(TKey key, out TValue value);

        public abstract TValue GetValue(TIdentity identity);
        public abstract bool TryGetValue(TIdentity identity, out TValue value);
    }
}