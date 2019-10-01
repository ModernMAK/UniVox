using UniVox.Types;
using UniVox.Types.Identities;
using UniVox.VoxelData;

namespace UniVox.Launcher
{
    public abstract class BaseBlockReference
    {
        public abstract void RenderPass(BlockAccessor blockData);
        
        public abstract ArrayMaterialIdentity GetMaterial();
        public abstract FaceSubMaterial GetSubMaterial();
    }
}