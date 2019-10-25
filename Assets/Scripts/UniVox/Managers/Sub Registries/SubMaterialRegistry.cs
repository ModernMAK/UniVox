using UniVox.Types;

namespace UniVox.Managers
{

    public class SubMaterialRegistry : BaseRegistry<SubMaterialKey, SubMaterialIdentity, int>
    {
        protected override SubMaterialIdentity CreateId(int index) => new SubMaterialIdentity(index);

        protected override int GetIndex(SubMaterialIdentity identity) => identity;
    }
}