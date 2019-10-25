using UnityEngine;
using UniVox.Managers.Game.Accessor;
using UniVox.Types;

namespace UniVox.Managers.Game.Structure
{
    public class MaterialRegistry : BaseRegistry<MaterialKey, MaterialIdentity, Material>
    {
        protected override MaterialIdentity CreateId(int index) => new MaterialIdentity(index);

        protected override int GetIndex(MaterialIdentity identity) => identity;
    }
}