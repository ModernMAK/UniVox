using UnityEngine;

namespace UniVox.Managers
{
    public class GameRegistry
    {
        public GameRegistry()
        {
            Meshes = new SimpleRegistry<Mesh>();
            Materials = new SimpleRegistry<Material>();
            Textures = new SimpleRegistry<Texture>();
            Sprites = new SimpleRegistry<Sprite>();
//            Sprites = new SpriteRegistry();
//            Blocks = new BlockRegistry();
//            Regions = new AtlasRegistry();
//            SubMaterials = new SubMaterialRegistry();
        }


        public IRegistry<string,int,Mesh> Meshes { get; }
        public IRegistry<string,int,Material> Materials { get; }

        public IRegistry<string,int,Texture> Textures { get; }
        
        public IRegistry<string,int,Sprite> Sprites { get; }
//        public AtlasRegistry Regions { get; }

//        public SubMaterialRegistry SubMaterials { get; }

//        public SpriteRegistry Sprites { get; }
//        public BlockRegistry Blocks { get; }
    }
}