using UnityEngine;
using UniVox.Managers;

[CreateAssetMenu(fileName = "BlockItem.Asset", menuName = "Data/Registry/Items/Block")]
public class BlockItemData : RegistryData
{
    public string ItemName;
    public BlockInfoData BlockInfo;
    public SpriteRegistryData Icon;
    

    public override int Register()
    {
        var blockId = BlockInfo.Register();
        var iconId = Icon.Register();
        var registry = GameData.Instance.Items;
        var itemInfo = new BlockItem(ItemName,blockId,iconId);
        return registry.Register(Key.GetFullKey(), itemInfo, RegisterOptions.ReturnExistingKey);
    }

    public override bool TryRegister(out int identity)
    {
        BlockInfo.TryRegister(out var blockId);
        Icon.TryRegister(out var iconId);
        var registry = GameData.Instance.Items;
        var itemInfo = new BlockItem(ItemName,blockId,iconId);
        return registry.TryRegister(Key.GetFullKey(), itemInfo, out identity, RegisterOptions.ReturnExistingKey);
    }
}