namespace UniVox.Core
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
    
    public interface INativeAccessorMap<in TKey, TAccessor> : IAccessorMap<TKey,TAccessor> where TAccessor : struct
    {
//        int Length { get; }
//        TAccessor this[int index] { get; }
//        TAccessor GetAccessor(int index);
    }
    public interface IAccessorMap<in TKey,TAccessor>
    {
        bool ContainsKey(TKey key);
        TAccessor this[TKey index] { get; }
        TAccessor GetAccessor(TKey index);

        bool TryGetAccessor(TKey key, out TAccessor accessor);
    }
    
}