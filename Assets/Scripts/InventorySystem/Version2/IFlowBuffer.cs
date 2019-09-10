namespace InventorySystem.Version2
{
    public interface IFlowBuffer 
    {
        int Stored { get; }
        int Capacity { get; }

        int Store(int store);
        int Release(int release);
    }
}