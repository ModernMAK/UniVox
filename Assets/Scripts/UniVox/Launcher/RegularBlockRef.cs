using System;
using UniVox.Managers.Game;
using UniVox.Types;
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

        public override ArrayMaterialId GetMaterial(BlockVariant blockData)
        {
            return _material;
        }

        public override FaceSubMaterial GetSubMaterial(BlockVariant blockData)
        {
            return _subMat;
        }
    }
}