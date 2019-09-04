namespace InventorySystem.Version2
{
    public interface IFlowSink
    {
        int Demand { get; }
        int FillDemand(int supply);
    }
}