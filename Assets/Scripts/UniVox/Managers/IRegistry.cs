namespace UniVox.Managers
{
    public interface IRegistry<in TKey, TValue>
    {
        TValue this[TKey key] { get; }
        bool Register(TKey key, TValue value);
        bool Unregister(TKey key);

        bool TryGetValue(TKey key, out TValue value);
        bool IsRegistered(TKey key);
    }
}