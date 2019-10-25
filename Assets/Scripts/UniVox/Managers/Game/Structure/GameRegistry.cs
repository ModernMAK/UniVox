namespace UniVox.Managers.Game.Structure
{
    public class GameRegistry
    {
        public GameRegistry()
        {
            Meshes = new MeshRegistry();
            Materials = new MaterialRegistry();
            Sprites = new SpriteRegistry();
            Blocks = new BlockRegistry();

//            Raw = new ModRegistry();
//            Mods = new ModRegistryAccessor(Raw);
//            ArrayMaterials = new ArrayMaterialRegistryAccessor(Mods);
//            Icons = new IconRegistryAccessor(Mods);
//            Meshes = new MeshRegistryAccessor(Mods);
//            Blocks = new BlockRegistryAccessor(Mods); //,this);
//            SubArrayMaterials = new SubArrayMaterialRegistryAccessor(ArrayMaterials);
////            NativeBlocks = new NativeHashMap<BlockIdentity, NativeBlock>(0, Allocator.Persistent);
//            Icons = new IconRegistryAccessor(Mods);
        }


        public MeshRegistry Meshes { get; }
        public MaterialRegistry Materials { get; }

        public SpriteRegistry Sprites { get; }
        public BlockRegistry Blocks { get; }

//        public NativeHashMap<BlockIdentity, NativeBlock> NativeBlocks { get; private set; }

//        public void UpdateNativeBlock()
//        {
//            NativeBlocks.DisposeEnumerable();
//            NativeBlocks = Blocks.CreateNativeBlockMap();
//        }
    }
}