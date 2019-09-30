using UniVox.Managers.Game;
using UniVox.VoxelData;

namespace UniVox.Launcher
{
    public class TopSideBlockRef : BaseBlockReference
    {
        public TopSideBlockRef(ArrayMaterialId materialId, int grass, int sideSub, int dirtSub)
        {
            _material = materialId;
            _subMaterial = FaceSubMaterial.CreateTopSideBot(grass, sideSub, dirtSub);
//                _grassSubMat = grass;
//                _grassSideSubMat = sideSub;
//                _dirtSubMat = dirtSub;
        }

        private readonly ArrayMaterialId _material;
        private FaceSubMaterial _subMaterial;

        //Cache to avoid dictionary lookups
//            private readonly int _grassSubMat;
//            private readonly int _grassSideSubMat;
//            private readonly int _dirtSubMat;


        public override void RenderPass(BlockAccessor blockData)
        {
            blockData.Material.Value = _material;

            blockData.SubMaterial.Value = _subMaterial;

//                renderData.SetSubMaterial(Direction.Down, _dirtSubMat);
//
//                renderData.SetSubMaterial(Direction.Left, _grassSideSubMat);
//                renderData.SetSubMaterial(Direction.Right, _grassSideSubMat);
//                renderData.SetSubMaterial(Direction.Forward, _grassSideSubMat);
//                renderData.SetSubMaterial(Direction.Backward, _grassSideSubMat);

//                renderData.Version.Dirty();
        }

        public override ArrayMaterialId GetMaterial(BlockVariant blockData)
        {
            return _material;
        }

        public override FaceSubMaterial GetSubMaterial(BlockVariant blockData)
        {
            return _subMaterial;
        }
    }
}