namespace InventorySystem
{
    public class FixedInventory : IInventory
    {
        public FixedInventory(int size)
        {
            Stacks = new IItemStack[size];
        }

        private IItemStack[] Stacks { get; }

        public IItemStack PeekStack(int index)
        {
            return Stacks[index];
        }

        public IItemStack GetStack(int index)
        {
            return SwapStack(index, AbstractItemStack.EmptyStack);
        }

        public void AddItems(int index, IItemStack stack)
        {
            Stacks[index].AddItems(stack);
        }

        public IItemStack SwapStack(int index, IItemStack stack)
        {
            var temp = Stacks[index];
            Stacks[index] = stack;
            return temp;
        }

        public int Size => Stacks.Length;
    }
}