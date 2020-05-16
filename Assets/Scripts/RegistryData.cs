using UnityEngine;
using UniVox.Managers;

public abstract class RegistryData<TData> : RegistryData
{
    [SerializeField] private TData _data;
    public TData Data => _data;


    public int Register(IRegistry<string, int, TData> registry) => registry.Register(Key.GetFullKey(), Data);

    public bool TryRegister(IRegistry<string, int, TData> registry, out int identity) =>
        registry.TryRegister(Key.GetFullKey(), Data, out identity);
    

}

public abstract class RegistryData : ScriptableObject
{
    [SerializeField] private RegistryKey _key;
    
    public RegistryKey Key => _key;

    public abstract int Register();
    public abstract bool TryRegister(out int identity);
}