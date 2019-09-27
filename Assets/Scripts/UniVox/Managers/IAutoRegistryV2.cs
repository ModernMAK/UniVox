namespace UniVox.Managers
{
    public interface IAutoRegistryV2<TKey, TValue> :
        IRegistryV2<TKey, TValue, IAutoReference<TKey,TValue>>
    {
        IAutoReference<TKey,TValue> this[int index] { get; }
        bool IsRegistered(int index);

        bool TryGetReference(int index, out IAutoReference<TKey,TValue> value);
    }
}