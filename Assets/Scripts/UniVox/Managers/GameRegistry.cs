namespace UniVox.Managers
{
    public class GameRegistry
    {
        public GameRegistry()
        {
            Meshes = new MeshRegistry();
            Materials = new MaterialRegistry();
            Sprites = new SpriteRegistry();
            Blocks = new BlockRegistry();
            Regions = new AtlasRegistry();
            SubMaterials = new SubMaterialRegistry();
        }


        public MeshRegistry Meshes { get; }
        public MaterialRegistry Materials { get; }

        public AtlasRegistry Regions { get; }

        public SubMaterialRegistry SubMaterials { get; }

        public SpriteRegistry Sprites { get; }
        public BlockRegistry Blocks { get; }
    }
}