using UnityEngine;
using UniVox.Types;

namespace UniVox.Managers
{
    public class AtlasRegistry : BaseRegistry<AtlasKey, AtlasIdentity, Rect>
    {
        protected override AtlasIdentity CreateId(int index) => new AtlasIdentity(index);

        protected override int GetIndex(AtlasIdentity identity) => identity;

    }
}