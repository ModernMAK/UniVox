namespace UniVox.Core.Types
{
    public interface INativeAccessorArray<out TAccessor> : IAccessorArray<TAccessor> where TAccessor : struct
    {
//        int Length { get; }
//        TAccessor this[int index] { get; }
//        TAccessor GetAccessor(int index);
    }

    public interface IAccessorArray<out TAccessor>
    {
        int Length { get; }
        TAccessor this[int index] { get; }
        TAccessor GetAccessor(int index);
    }

    public interface INativeAccessorMap<in TKey, TAccessor> : IAccessorMap<TKey, TAccessor> where TAccessor : struct
    {
//        int Length { get; }
//        TAccessor this[int index] { get; }
//        TAccessor GetAccessor(int index);
    }

    public interface IAccessorMap<in TKey, TAccessor>
    {
        TAccessor this[TKey index] { get; }
        bool ContainsKey(TKey key);
        TAccessor GetAccessor(TKey index);

        bool TryGetAccessor(TKey key, out TAccessor accessor);
    }
}