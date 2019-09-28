namespace UniVox.Managers
{
    public interface IAutoReference<out TKey, out TValue> : IKeyReference<TKey, TValue>, IIdentityReference<TValue>
    {
    }
}