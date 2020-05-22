namespace UniVox.Utility
{
    public interface IIndexConverter<T>
    {
        T Size { get; }
        int Flatten(T value);
        T Expand(int value);
    }
}