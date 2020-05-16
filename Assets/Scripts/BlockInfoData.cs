using UnityEngine;

[CreateAssetMenu(fileName = "BlockInfo.Asset", menuName = "Data/Registry/Block")]
public class BlockInfoData : RegistryData
{
    public SpriteRegistryData Icon;
    public MaterialRegistryData Material;


    public override int Register()
    {
        var iconId = Icon.Register();
        var materialID = Material.Register();
        var blockInfo = new BlockInfo(materialID,iconId);
        var registry = GameData.Instance.Blocks;
        return registry.Register(Key.GetFullKey(), blockInfo);
    }

    public override bool TryRegister(out int identity)
    {
        Icon.TryRegister(out var iconId);
        Material.TryRegister(out var materialID);
        var blockInfo = new BlockInfo(materialID,iconId);
        var registry = GameData.Instance.Blocks;
        return registry.TryRegister(Key.GetFullKey(), blockInfo, out identity);
    }
}