public interface IItemStack : IItem
{
    IItem GetItem();
    IItemStack GetItems(int count);
}