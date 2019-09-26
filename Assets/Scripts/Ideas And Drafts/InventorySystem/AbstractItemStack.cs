using System;

namespace InventorySystem
{
    public class AbstractItemStack : IItemStack
    {
        private AbstractItemStack() : this(Guid.Empty, 1, 1)
        {
        }


        public AbstractItemStack(Guid guid, int capacity = 1) : this(guid, 1, capacity)
        {
        }

        public AbstractItemStack(Guid guid, int count, int capacity)
        {
#if INVENTORY_SAFETY_CHECK
            if (capacity <= 0)
                throw new ArgumentException($"Capacity ({capacity}) must be greater than 0!", nameof(capacity));

            if (count <= 0)
                throw new ArgumentException($"Count ({count}) must be greater than 0!", nameof(count));

            if (count > capacity)
                throw new ArgumentException($"Count ({count}) must be less than or equal to the Capacity ({capacity})!",
                    nameof(count));
#endif

            ItemId = guid;
            Count = count;
            Capacity = capacity;
        }

        public static AbstractItemStack EmptyStack => new AbstractItemStack();


        private int SpaceRemaining => Capacity - Count;


        public IItemStack GetItems(int requested)
        {
            if (requested <= 0) return EmptyStack;

            var removed = requested >= Count ? Count : requested;

            Count -= removed;
            return new AbstractItemStack(ItemId, removed, Capacity);
        }


        public void AddItems(IItemStack items)
        {
            if (items.ItemId.Equals(ItemId))
            {
                //We dont expose a way to remove items, so we have to do it via GetItems
                var stack = items.GetItems(SpaceRemaining);
                Count += stack.Count;
            }
        }

        public Guid ItemId { get; }

        public int Count { get; private set; }

        public int Capacity { get; }
    }
}