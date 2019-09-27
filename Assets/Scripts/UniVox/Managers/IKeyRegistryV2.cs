namespace UniVox.Managers
{
    public interface IKeyRegistryV2<TKey, TValue, TReference> : IRegistryV2<TKey, TValue, IKeyReference<TKey, TValue>>
    {
    }
}