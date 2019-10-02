using UnityEngine;
using UniVox.Managers.Game.Accessor;
using UniVox.Types;
using UniVox.Types.Exceptions;
using UniVox.Types.Identities;
using UniVox.Types.Keys;
using UniVox.VoxelData;

namespace UniVox.Launcher
{
    public class RegularBlockRef : BaseBlockReference
    {
        public RegularBlockRef(ArrayMaterialIdentity materialIdentity, BlockKey blockKey, IconKey iconKey, int subMat = 0)
        {
            _material = materialIdentity;
            _subMat = FaceSubMaterial.CreateAll(subMat);
            BlockKey = blockKey;
            IconKey = iconKey;
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
    }
}