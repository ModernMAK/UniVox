using UniVox.Entities.Systems.Registry;

namespace UniVox.Entities.Systems.Surrogate
{
    public class ModRegistryRecordSurrogate : NamedValue<ModRegistryRecord>
    {

        public MeshRegistryRecordSurrogate[] Meshes;
        public MaterialRegistryRecordSurrogate[] Materials;
        public BlockRegistryRecordSurogate[] Blocks;
        public EntityRegistryRecordSurrogate[] Entities;
    }
}