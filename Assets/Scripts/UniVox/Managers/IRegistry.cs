namespace InventorySystem
{
    public interface IRegistry<in TKey, TValue>
    {
        bool Register(TKey key, TValue value);
        bool Unregister(TKey key);

        TValue this[TKey key] { get; }

        bool TryGetValue(TKey key, out TValue value);
        bool IsRegistered(TKey key);
    }
}