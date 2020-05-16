using UnityEngine;

[CreateAssetMenu(menuName = "Data/Registry/Texture", fileName = "Texture.asset")]
public class TextureRegistryData : RegistryData<Texture>
{
    public override int Register()
    {
        var registry = GameData.Instance.Textures;
        return Register(registry);
    }

    public override bool TryRegister(out int identity)
    {
        var registry = GameData.Instance.Textures;
        return TryRegister(registry, out identity);
    }
}