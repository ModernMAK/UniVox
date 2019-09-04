namespace InventorySystem.Version2
{
    public interface IFlowBuffer : IFlowSource, IFlowSink
    {
        int Capacity { get; }
    }
}