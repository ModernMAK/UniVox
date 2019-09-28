namespace UniVox.Managers.Generic
{
    public interface IIndexedRegistry<in TKey,TValue> 
    {
        TValue this[int index] { get; }
        bool Register(TKey key, TValue value, out int index);

        bool TryGetValue(int index, out TValue value);
        bool TryGetIndex(TKey key, out int index);
        bool IsRegistered(int index);
    }
}