namespace UniVox.Managers
{
    public interface IIdentityRegistryV2<TValue> : IRegistryV2<int, TValue, IIdentityReference<TValue>>
    {
    }
}