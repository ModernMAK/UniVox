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
    public interface IIndexedRegistry<in TKey,TValue> 
    {
        TValue this[int index] { get; }
        bool Register(TKey key, TValue value, out int index);

        bool TryGetValue(int index, out TValue value);
        bool TryGetIndex(TKey key, out int index);
        bool IsRegistered(int index);
    }
    public interface IIndexedRegistryKey<out TValue> 
    {
        int Index { get; }
        TValue Value { get; }
    }

}