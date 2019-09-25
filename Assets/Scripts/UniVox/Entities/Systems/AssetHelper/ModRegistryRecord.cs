namespace UniVox.Entities.Systems.Registry
{
    public class ModRegistryRecord
    {
        public ModRegistryRecord()
        {
            Meshes = new MeshRegistry();
            Materials = new MaterialRegistry();
            Blocks = new BlockRegistry();
            Entities = new EntityRegistry();
        }

        public MeshRegistry Meshes { get; }
        public MaterialRegistry Materials { get; }
        public BlockRegistry Blocks { get; }
        public EntityRegistry Entities { get; }
    }
}