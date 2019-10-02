using UnityEngine;
using UniVox.Managers.Game;
using UniVox.Managers.Game.Accessor;
using UniVox.Types;
using UniVox.Types.Exceptions;
using UniVox.Types.Identities;
using UniVox.Types.Keys;
using UniVox.VoxelData;

namespace UniVox.Launcher
{
    public class TopSideBlockRef : BaseBlockReference
    {
        public TopSideBlockRef(ArrayMaterialIdentity materialIdentity, BlockKey blockKey, IconKey iconKey, int grass, int sideSub, int dirtSub)
        {
            _material = materialIdentity;
            _subMaterial = FaceSubMaterial.CreateTopSideBot(grass, sideSub, dirtSub);
//                _grassSubMat = grass;
//                _grassSideSubMat = sideSub;
//                _dirtSubMat = dirtSub;
            BlockKey = blockKey;
            IconKey = iconKey;
        }

        private readonly ArrayMaterialIdentity _material;
        private FaceSubMaterial _subMaterial;

        //Cache to avoid dictionary lookups
//            private readonly int _grassSubMat;
//            private readonly int _grassSideSubMat;
//            private readonly int _dirtSubMat;

        public override ArrayMaterialIdentity GetMaterial()
        {
            return _material;
        }

        public override FaceSubMaterial GetSubMaterial()
        {
            return _subMaterial;
        }

        public override Sprite GetBlockIcon()
        {
            if (!GameManager.Registry.Icons.TryGetValue(IconKey, out var sprite))
                throw new AssetNotFoundException(nameof(IconKey), IconKey.ToString());
            return sprite;
        }

        public override BlockIdentity GetBlockId()
        {
            if (!GameManager.Registry.Blocks.TryGetIdentity(BlockKey, out var blockIdentity))
                throw new AssetNotFoundException(nameof(BlockKey), BlockKey.ToString());
            return blockIdentity;
        }

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