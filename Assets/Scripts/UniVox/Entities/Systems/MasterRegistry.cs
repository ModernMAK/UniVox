using InventorySystem;
using UnityEngine;

public class MasterRegistry
{
    public MasterRegistry()
    {
        Mesh = new NamedRegistry<Mesh>();
        Material = new NamedRegistry<Material>();
        Icon = new NamedRegistry<Sprite>();
    }

    public NamedRegistry<Mesh> Mesh { get; }
    public NamedRegistry<Material> Material { get; }
    public NamedRegistry<Sprite> Icon { get; }
}