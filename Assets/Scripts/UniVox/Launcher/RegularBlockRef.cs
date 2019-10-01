using UniVox.Managers.Game;
using UniVox.Types;
using UniVox.Types.Identities;
using UniVox.VoxelData;

namespace UniVox.Launcher
{
    public class RegularBlockRef : BaseBlockReference
    {
        public RegularBlockRef(ArrayMaterialIdentity materialIdentity, int subMat = 0)
        {
            _material = materialIdentity;
            _subMat = FaceSubMaterial.CreateAll(subMat);
        }

        private readonly ArrayMaterialIdentity _material;
        private readonly FaceSubMaterial _subMat;


        public override void RenderPass(BlockAccessor blockData)
        {
            blockData.Material.Value = _material;
            blockData.SubMaterial.Value = _subMat;
        }

        public override ArrayMaterialIdentity GetMaterial()
        {
            return _material;
        }

        public override FaceSubMaterial GetSubMaterial()
        {
            return _subMat;
        }
    }
}