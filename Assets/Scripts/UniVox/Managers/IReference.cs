namespace UniVox.Managers
{
    public interface IReference<out TValue>
    {
        TValue Value { get; }
    }
}