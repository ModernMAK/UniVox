using System.Collections.Generic;
using UniVox.Entities.Systems.Registry;
using UniVox.Entities.Systems.Surrogate;

namespace UniVox.Entities.Systems
{
    public static class SurrogateProxyModHelpers
    {
        public static void RegistrySurrogates(this GameRegistry registry, ModId id,
            ModRegistryRecordSurrogate surrogate)
        {
            registry.Mods[id].RegistrySurrogates(surrogate);
        }


        public static void RegistrySurrogates(this ModRegistry.Record modRegistry, ModRegistryRecordSurrogate surrogate)
        {
            foreach (var matSur in surrogate.Atlas)
            {
                var mat = matSur.Value;
                var regions = matSur.Regions;

                var atlasMaterialRef = modRegistry.Atlases.Register(matSur.Name, mat);
                foreach (var region in regions)
                {
                    atlasMaterialRef.Value.Regions.Register(region.Name, region.Value);
                }
            }

            foreach (var matSur in surrogate.Materials)
            {
                var mat = matSur.Value;
                var subMats = matSur.SubMaterials;

                var arrayMaterialRef = modRegistry.Materials.Register(matSur.Name, mat);
                foreach (var subMat in subMats)
                {
                    modRegistry.Materials[arrayMaterialRef].SubMaterials.Register(subMat.Name, subMat.Value);
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