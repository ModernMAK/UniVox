namespace UniVox.Managers
{
    public interface IRegistryV2<in TKey, in TValue, TReference> where TReference : IReference<TValue>
    {
        int Count { get; }
        TReference this[TKey key] { get; }
        bool IsRegistered(TKey key);
        bool Register(TKey key, TValue value);
        bool Register(TKey key, TValue value, out TReference reference);
        bool TryGetReference(TKey key, out TReference value);
    }
}