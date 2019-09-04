namespace InventorySystem
{
    public interface IInventory
    {
        IItemStack PeekStack(int index);
        IItemStack GetStack(int index);
        void AddItems(int index, IItemStack stack);
        IItemStack SwapStack(int index, IItemStack stack);

        int Size { get; }
    }
}