using UnityEngine;

[CreateAssetMenu(menuName = "Data/Registry/Sprite", fileName = "Texture.sprrd.asset")]
public class SpriteRegistryData : RegistryData<Sprite>
{
    public override int Register()
    {
        var registry = GameData.Instance.Sprites;
        return Register(registry);
    }

    public override bool TryRegister(out int identity)
    {
        var registry = GameData.Instance.Sprites;
        return TryRegister(registry, out identity);
    }
}