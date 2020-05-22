using UnityEngine;

[CreateAssetMenu(fileName = "ToolItem.Asset", menuName = "Data/Registry/Items/Tool")]
public class ToolItemData : RegistryData
{
    public string ItemName;
    public ToolType Tool;
    public SpriteRegistryData Icon;
    

    public override int Register()
    {
        var iconId = Icon.Register();
        var registry = GameData.Instance.Items;
        var itemInfo = new ToolItem(ItemName,Tool,iconId);
        return registry.Register(Key.GetFullKey(), itemInfo);
    }

    public override bool TryRegister(out int identity)
    {
        Icon.TryRegister(out var iconId);
        var registry = GameData.Instance.Items;
        var itemInfo = new ToolItem(ItemName,Tool,iconId);
        return registry.TryRegister(Key.GetFullKey(), itemInfo, out identity);
    }
}