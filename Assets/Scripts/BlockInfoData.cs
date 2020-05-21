using UnityEngine;

[CreateAssetMenu(fileName = "BlockInfo.Asset", menuName = "Data/Registry/Block")]
public class BlockInfoData : RegistryData
{
    public MaterialRegistryData Material;


    public override int Register()
    {
        var materialId = Material.Register();
        var blockInfo = new BlockInfo(materialId);
        var registry = GameData.Instance.Blocks;
        return registry.Register(Key.GetFullKey(), blockInfo);
    }

    public override bool TryRegister(out int identity)
    {
        Material.TryRegister(out var materialId);
        var blockInfo = new BlockInfo(materialId);
        var registry = GameData.Instance.Blocks;
        return registry.TryRegister(Key.GetFullKey(), blockInfo, out identity);
    }
}