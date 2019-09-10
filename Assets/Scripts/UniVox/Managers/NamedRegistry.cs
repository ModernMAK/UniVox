namespace InventorySystem
{
    /// <summary>
    /// Utility registry lookup via string
    /// </summary>
    public class NamedRegistry<TValue> : AutoRegistry<string, TValue>
    {
    }
}