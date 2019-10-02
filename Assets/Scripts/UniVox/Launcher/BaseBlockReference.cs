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

        public NativeBaseBlockReference GetNative()
        {
            return new NativeBaseBlockReference(this);
        }
    }

    public struct NativeBaseBlockReference
    {
        public NativeBaseBlockReference(BaseBlockReference blockRef)
        {
            Material = blockRef.GetMaterial();
            SubMaterial = blockRef.GetSubMaterial();
        }

        public ArrayMaterialIdentity Material;
        public FaceSubMaterial SubMaterial;
    }
}