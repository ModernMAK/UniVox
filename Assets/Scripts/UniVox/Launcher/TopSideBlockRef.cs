using UniVox.Managers.Game;
using UniVox.Types.Identities;
using UniVox.VoxelData;

namespace UniVox.Launcher
{
    public class TopSideBlockRef : BaseBlockReference
    {
        public TopSideBlockRef(ArrayMaterialIdentity materialIdentity, int grass, int sideSub, int dirtSub)
        {
            _material = materialIdentity;
            _subMaterial = FaceSubMaterial.CreateTopSideBot(grass, sideSub, dirtSub);
//                _grassSubMat = grass;
//                _grassSideSubMat = sideSub;
//                _dirtSubMat = dirtSub;
        }

        private readonly ArrayMaterialIdentity _material;
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
    }
}