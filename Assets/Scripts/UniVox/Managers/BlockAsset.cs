using System;
using UnityEngine;
using UniVox.Launcher;
using UniVox.Managers.Game;
using UniVox.Managers.Game.Accessor;
using UniVox.Managers.Game.Structure;
using UniVox.Types.Keys;

namespace UniVox.Managers
{
    public enum RenderType
    {
        SideTopBottom, All
    }
    
    [CreateAssetMenu(menuName = "Custom Assets/Block")]
    public class BlockAsset : ScriptableObject
    {
        public Sprite icon;
        public Material material;
        public String blockName;
        public RenderType renderType;
        public int top, side, bottom, all;

        public BlockKey blockKey;

        // enum (render type)
        // 3 nums (side top bottom, all, none) for texture index
        // function create block reference in basegamemod

        public void CreateBlockReference()
        {
            var materialKey = new ArrayMaterialKey(BaseGameMod.ModPath, blockName);
            GameManager.Registry.ArrayMaterials.Register(materialKey, material, out var materialIdentity);
            
            var iconKey = new IconKey(BaseGameMod.ModPath, blockName);
            GameManager.Registry.Icons.Register(iconKey, icon, out var iconIdentity);
            
            blockKey = new BlockKey(BaseGameMod.ModPath, blockName);
            BaseBlockReference blockReference;
            switch (renderType)
            {
                case RenderType.SideTopBottom:
                    blockReference = new TopSideBlockRef(materialIdentity, blockKey, iconKey, top, side, bottom);
                    break;
                case RenderType.All:
                    blockReference = new RegularBlockRef(materialIdentity, blockKey, iconKey, all);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            GameManager.Registry.Blocks.Register(blockKey, blockReference, out var identity);
        }
    }
}