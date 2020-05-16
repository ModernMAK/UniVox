using UnityEngine;

[CreateAssetMenu(menuName = "Data/Registry/Mesh",fileName = "Mesh.mshrd.asset")]
public class MeshRegistryData : RegistryData<Mesh>
{
    public override int Register()
    {
        var registry = GameData.Instance.Meshes;
        return Register(registry);
    }

    public override bool TryRegister(out int identity)
    {
        var registry = GameData.Instance.Meshes;
        return TryRegister(registry, out identity);
    }
}