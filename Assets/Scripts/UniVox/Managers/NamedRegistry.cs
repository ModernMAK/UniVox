namespace UniVox.Managers
{
    /// <summary>
    /// Utility registry lookup via string
    /// </summary>
    public class NamedRegistry<TValue> : AutoRegistry<string, TValue>
    {
    }

    /// <summary>
    /// Utility registry lookup via string
    /// </summary>
    public class NamedRegistryV2<TValue> : AutoRegistryV2<string, TValue>
    {
    }

}