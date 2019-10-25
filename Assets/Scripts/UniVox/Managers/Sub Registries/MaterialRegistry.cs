using UnityEngine;
using UniVox.Types;

namespace UniVox.Managers
{
    public class MaterialRegistry : BaseRegistry<MaterialKey, MaterialIdentity, Material>
    {
        protected override MaterialIdentity CreateId(int index) => new MaterialIdentity(index);

        protected override int GetIndex(MaterialIdentity identity) => identity;
    }
}