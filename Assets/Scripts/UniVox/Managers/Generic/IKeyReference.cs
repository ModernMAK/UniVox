namespace UniVox.Managers
{
    public interface IKeyReference<out TKey, out TValue> : IReference<TValue>
    {
        TKey Key { get; }
    }
}