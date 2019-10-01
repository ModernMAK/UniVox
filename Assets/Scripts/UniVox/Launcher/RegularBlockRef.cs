using UniVox.Managers.Game;
using UniVox.VoxelData;

namespace UniVox.Launcher
{
    public class RegularBlockRef : BaseBlockReference
    {
        public RegularBlockRef(ArrayMaterialId materialId, int subMat = 0)
        {
            _material = materialId;
            _subMat = FaceSubMaterial.CreateAll(subMat);
        }

        private readonly ArrayMaterialId _material;
        private readonly FaceSubMaterial _subMat;


        public override void RenderPass(BlockAccessor blockData)
        {
            blockData.Material.Value = _material;
            blockData.SubMaterial.Value = _subMat;
        }
    }
}