namespace UniVox.Core.Types
{
    public interface INativeDataArray<TData> : IDataArray<TData> where TData : struct
    {
    }

    public interface IDataArray<TData>
    {
        int Length { get; }
        TData GetData(int index);
        void SetData(int index, TData value);
    }
}