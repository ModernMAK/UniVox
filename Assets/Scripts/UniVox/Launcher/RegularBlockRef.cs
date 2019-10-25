using UnityEngine;
using UniVox.Managers.Game.Accessor;
using UniVox.Types;
using UniVox.Types.Exceptions;

namespace UniVox.Launcher
{
    public class RegularBlockRef : BaseBlockReference
    {
        private readonly MaterialIdentity _material;
        private readonly FaceSubMaterial _subMat;

        public RegularBlockRef(MaterialIdentity materialIdentity, BlockKey blockKey, SpriteKey spriteKey,
            int subMat = 0)
        {
            _material = materialIdentity;
            _subMat = FaceSubMaterial.CreateAll(subMat);
            BlockKey = blockKey;
            SpriteKey = spriteKey;
        }


        public override MaterialIdentity GetMaterial()
        {
            return _material;
        }

        public override FaceSubMaterial GetSubMaterial()
        {
            return _subMat;
        }

        public override Sprite GetBlockIcon()
        {
            if (!GameManager.Registry.Sprites.TryGetValue(SpriteKey, out var sprite))
                throw new AssetNotFoundException(nameof(SpriteKey), SpriteKey.ToString());
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