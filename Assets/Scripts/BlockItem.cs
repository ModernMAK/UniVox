using UnityEngine;

public struct BlockItem : IItem, IUsable
{
    public BlockItem(string name, int blockId, int iconId)
    {
        _name = name;
        _block = blockId;
        _icon = iconId;

    }
    private readonly string _name;
    private int _block;
    private readonly int _icon;
    public string GetName()
    {
        return _name;
    }

    public Sprite GetIcon()
    {
        return GameData.Instance.Sprites[_icon];
    }

    public void Use()
    {
        //TODO
        //Raycast & find neighbor
        //Check occupancy
        //Place block if unoccupied
    }
}