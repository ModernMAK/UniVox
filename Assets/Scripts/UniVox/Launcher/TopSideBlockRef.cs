using UnityEngine;
using UniVox.Managers.Game.Accessor;
using UniVox.Types;
using UniVox.Types.Exceptions;
using UniVox.Types.Identities;
using UniVox.Types.Keys;

namespace UniVox.Launcher
{
    public class TopSideBlockRef : BaseBlockReference
    {
        private readonly ArrayMaterialIdentity _material;
        private readonly FaceSubMaterial _subMaterial;

        public TopSideBlockRef(ArrayMaterialIdentity materialIdentity, BlockKey blockKey, IconKey iconKey, int top,
            int side, int bot)
        {
            _material = materialIdentity;
            _subMaterial = FaceSubMaterial.CreateTopSideBot(top, side, bot);
//                _grassSubMat = top;
//                _grassSideSubMat = side;
//                _dirtSubMat = bot;
            BlockKey = blockKey;
            IconKey = iconKey;
        }

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
    }
}