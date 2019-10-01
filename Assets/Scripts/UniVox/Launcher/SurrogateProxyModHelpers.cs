using System.Collections.Generic;
using UniVox.Launcher.Surrogate;
using UniVox.Managers.Game.Structure;
using UniVox.Types.Identities;

namespace UniVox.Launcher
{
    public static class SurrogateProxyModHelpers
    {
        public static void RegistrySurrogates(this GameRegistry registry, ModIdentity identity,
            ModRegistryRecordSurrogate surrogate)
        {
            registry.Mods[identity].RegistrySurrogates(surrogate);
        }


        public static void RegistrySurrogates(this ModRegistry.Record modRegistry, ModRegistryRecordSurrogate surrogate)
        {
            foreach (var matSur in surrogate.Atlas)
            {
                var mat = matSur.Value;
                var regions = matSur.Regions;

                var atlasMaterialIndex = modRegistry.Atlases.Register(matSur.Name, mat);
                foreach (var region in regions)
                {
                    modRegistry.Atlases[atlasMaterialIndex].Regions.Register(region.Name, region.Value);
                }
            }

            foreach (var matSur in surrogate.Materials)
            {
                var mat = matSur.Value;
                var subMats = matSur.SubMaterials;

                var arrayMaterialIndex = modRegistry.Materials.Register(matSur.Name, mat);
                foreach (var subMat in subMats)
                {
                    modRegistry.Materials[arrayMaterialIndex].SubMaterials.Register(subMat.Name, subMat.Value);
                }
            }
        }

        public static bool TryGet<T, U>(this IList<T> blockReg, string name, out U record) where T : NamedValue<U>
        {
            for (var i = 0; i < blockReg.Count; i++)
            {
                var item = blockReg[i];
                if (item.Name.Equals(name))
                {
                    record = item.Value;
                    return true;
                }
            }

            record = default;
            return false;
        }
    }
}