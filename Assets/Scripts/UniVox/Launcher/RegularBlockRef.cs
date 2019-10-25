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
        private readonly SpriteIdentity _spriteIdentity;

        public RegularBlockRef(MaterialIdentity materialIdentity, SpriteIdentity spriteKey,
            int subMat = 0)
        {
            _material = materialIdentity;
            _subMat = FaceSubMaterial.CreateAll(subMat);
            _spriteIdentity = spriteKey;
        }


        public override MaterialIdentity GetMaterial()
        {
            return _material;
        }

        public override FaceSubMaterial GetSubMaterial()
        {
            return _subMat;
        }

        public override Sprite GetBlockIcon() => GameManager.Registry.Sprites[_spriteIdentity];

    }
}