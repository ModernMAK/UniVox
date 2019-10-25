using System;
using UnityEngine;
using UniVox.Launcher;
using UniVox.Managers.Game.Accessor;
using UniVox.Types;

namespace UniVox.Managers
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

        //NOTE TO FALCON - I was confused as to why this was public, since it wasnt used at all for the creation script
        //I made the variable into a field (a getter and setter, in this case it uses an automatic variable when it compiles)
        public BlockKey Key { get; private set; }

        // enum (render type)
        // 3 nums (side top bottom, all, none) for texture index
        // function create block reference in basegamemod

        public void CreateBlockReference()
        {
            var materialKey = new MaterialKey(BaseGameMod.ModPath, blockName);
            var materialIdentity = GameManager.Registry.Materials.Register(materialKey, material);

            var iconKey = new SpriteKey(BaseGameMod.ModPath, blockName);
            var iconIdentity = GameManager.Registry.Sprites.Register(iconKey, icon);

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

            var identity = GameManager.Registry.Blocks.Register(Key, blockReference);

            GameManager.NativeRegistry.UpdateBlocksFromRegistry(GameManager.Registry.Blocks);
        }
    }
}