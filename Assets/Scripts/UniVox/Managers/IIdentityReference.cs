namespace UniVox.Managers
{
    public interface IIdentityReference<out TValue> : IReference<TValue>
    {
        int Id { get; }
    }
}