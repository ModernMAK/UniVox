using UnityEngine;
using UniVox.Managers.Game.Accessor;
using UniVox.Types;

namespace UniVox.Managers.Game.Structure
{
    public class MeshRegistry : BaseRegistry<MeshKey, MeshIdentity, Mesh>
    {
        protected override MeshIdentity CreateId(int index) => new MeshIdentity(index);

        protected override int GetIndex(MeshIdentity identity) => identity;
    }
}