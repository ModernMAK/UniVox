using UnityEngine;

[CreateAssetMenu(menuName = "Data/Registry/Material", fileName = "Material.asset")]
public class MaterialRegistryData : RegistryData<Material>
{
    public override int Register()
    {
        var registry = GameData.Instance.Materials;
        return Register(registry);
    }

    public override bool TryRegister(out int identity)
    {
        var registry = GameData.Instance.Materials;
        return TryRegister(registry, out identity);
    }
}