using System;
using UnityEngine;
using UniVox.Launcher;
using UniVox.Types;

namespace UniVox.Managers.Registration
{

    public enum RenderType
    {
        SideTopBottom,
        All
    }


    [CreateAssetMenu(menuName = "Custom Assets/Value")]
    public class BlockAsset : ScriptableObject
    {
        public string blockName;
        public Sprite icon;
        public Material material;
        public RenderType renderType;
        public int top, side, bottom, all;


        // enum (render type)
        // 3 nums (side top bottom, all, none) for texture index
        // function create block reference in basegamemod

        public BlockIdentity CreateBlockReference()
        {
            var materialKey = new MaterialKey(BaseGameMod.ModPath, blockName);
            var materialIdentity = GameManager.Registry.Materials.Register(materialKey, material);

            var iconKey = new SpriteKey(BaseGameMod.ModPath, blockName);
            var iconIdentity = GameManager.Registry.Sprites.Register(iconKey, icon);

            var key = new BlockKey(BaseGameMod.ModPath, blockName);
            AbstractBlock block;
            switch (renderType)
            {
                case RenderType.SideTopBottom:
                    block = new TopSideBlockRef(materialIdentity, iconIdentity, top, side, bottom);
                    break;
                case RenderType.All:
                    block = new RegularBlockRef(materialIdentity, iconIdentity, all);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var identity = GameManager.Registry.Blocks.Register(key, block);

            GameManager.NativeRegistry.UpdateBlocksFromRegistry(GameManager.Registry.Blocks);
            return identity;
        }
    }
}