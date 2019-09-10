namespace InventorySystem.Version2
{
    public interface IFlowSource
    {
        int Supply { get;}
        int DrainSupply(int demand);
    }
}