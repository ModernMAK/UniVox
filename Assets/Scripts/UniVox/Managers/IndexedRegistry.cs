namespace InventorySystem
{
    /// <summary>
    /// Utility registry lookup via integer
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class IndexedRegistry<TValue> : Registry<int, TValue>
    {
    }
}