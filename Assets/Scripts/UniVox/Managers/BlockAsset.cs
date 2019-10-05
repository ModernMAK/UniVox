using System;
using UnityEngine;
using UniVox.Launcher;
using UniVox.Managers.Game.Accessor;
using UniVox.Types.Keys;

namespace UniVox.Managers
{
    public enum RenderType
    {
        SideTopBottom,
        All
    }

    [CreateAssetMenu(menuName = "Custom Assets/Block")]
    public class BlockAsset : ScriptableObject
    {
        
        //NOTE TO FALCON - I was confused as to why this was public, since it wasnt used at all for the creation script
        //I made the variable into a field (a getter and setter, in this case it uses an automatic variable when it compiles)
        public BlockKey Key { get; private set; }
        public string blockName;
        public Sprite icon;
        public Material material;
        public RenderType renderType;
        public int top, side, bottom, all;

        // enum (render type)
        // 3 nums (side top bottom, all, none) for texture index
        // function create block reference in basegamemod

        public void CreateBlockReference()
        {
            var materialKey = new ArrayMaterialKey(BaseGameMod.ModPath, blockName);
            GameManager.Registry.ArrayMaterials.Register(materialKey, material, out var materialIdentity);

            var iconKey = new IconKey(BaseGameMod.ModPath, blockName);
            GameManager.Registry.Icons.Register(iconKey, icon, out var iconIdentity);

            Key = new BlockKey(BaseGameMod.ModPath, blockName);
            BaseBlockReference blockReference;
            switch (renderType)
            {
                case RenderType.SideTopBottom:
                    blockReference = new TopSideBlockRef(materialIdentity, Key, iconKey, top, side, bottom);
                    break;
                case RenderType.All:
                    blockReference = new RegularBlockRef(materialIdentity, Key, iconKey, all);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GameManager.Registry.Blocks.Register(Key, blockReference, out var identity);

            GameManager.NativeRegistry.UpdateBlocksFromRegistry(GameManager.Registry.Blocks);
        }
    }
}