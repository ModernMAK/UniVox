using UnityEngine;
using UniVox.Types;

namespace UniVox.Managers
{
    public class SpriteRegistry : BaseRegistry<SpriteKey, SpriteIdentity, Sprite>
    {
        protected override SpriteIdentity CreateId(int index) => new SpriteIdentity(index);

        protected override int GetIndex(SpriteIdentity identity) => identity;
    }
}