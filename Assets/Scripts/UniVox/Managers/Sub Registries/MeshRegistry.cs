using UnityEngine;
using UniVox.Types;

namespace UniVox.Managers
{
    public class MeshRegistry : BaseRegistry<MeshKey, MeshIdentity, Mesh>
    {
        protected override MeshIdentity CreateId(int index) => new MeshIdentity(index);

        protected override int GetIndex(MeshIdentity identity) => identity;
    }
}