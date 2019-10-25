using UnityEngine;
using UniVox.Managers.Game.Accessor;
using UniVox.Types;
using UniVox.Types.Exceptions;

namespace UniVox.Launcher
{
    public class TopSideBlockRef : BaseBlockReference
    {
        private readonly MaterialIdentity _material;
        private readonly FaceSubMaterial _subMaterial;
        private readonly SpriteIdentity _spriteIdentity;

        public TopSideBlockRef(MaterialIdentity materialIdentity, SpriteIdentity spriteKey, int top,
            int side, int bot)
        {
            _material = materialIdentity;
            _subMaterial = FaceSubMaterial.CreateTopSideBot(top, side, bot);
//                _grassSubMat = top;
//                _grassSideSubMat = side;
//                _dirtSubMat = bot;
            _spriteIdentity = spriteKey;
        }

        //Cache to avoid dictionary lookups
//            private readonly int _grassSubMat;
//            private readonly int _grassSideSubMat;
//            private readonly int _dirtSubMat;

        public override MaterialIdentity GetMaterial()
        {
            return _material;
        }

        public override FaceSubMaterial GetSubMaterial()
        {
            return _subMaterial;
        }

        public override Sprite GetBlockIcon() => GameManager.Registry.Sprites[_spriteIdentity];

    }
}