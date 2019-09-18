namespace FlowSystem
{
    public interface IFlowSource
    {
        int Supply { get; }
        int DrainSupply(int demand);
    }
}