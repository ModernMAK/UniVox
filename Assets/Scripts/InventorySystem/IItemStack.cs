using System;

namespace InventorySystem
{
    public interface IItemStack
    {
        /// <summary>
        /// Gets a new item stack by emptying this stack.
        /// </summary>
        /// <param name="requested">The requested size of the new stack.</param>
        /// <returns></returns>
        IItemStack GetItems(int requested);


//        bool IsValid(IItemStack items);
        void AddItems(IItemStack items);
        Guid ItemId { get; }
        int Count { get; }
        int Capacity { get; }
    }
}