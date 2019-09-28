using UniVox.Core.Types;
using UniVox.Rendering.ChunkGen.Jobs;

namespace UniVox.Entities.Systems
{
    public class GrassBlockRef : BaseBlockReference
    {
        public GrassBlockRef(MaterialId materialId, int grass, int sideSub, int dirtSub)
        {
            _material = materialId;
            _subMaterial = FaceSubMaterial.CreateTopSideBot(grass, sideSub, dirtSub);
//                _grassSubMat = grass;
//                _grassSideSubMat = sideSub;
//                _dirtSubMat = dirtSub;
        }

        private readonly MaterialId _material;
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