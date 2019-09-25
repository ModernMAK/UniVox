using UnityEngine;

namespace UniVox.Entities.Systems.Registry
{
    public class MaterialRegistryRecord
    {
        public MaterialRegistryRecord(Material material)
        {
            Value = material;
            Regions = new AtlasRegionRegistry();
        }

        public Material Value { get; }
        public AtlasRegionRegistry Regions { get; }

        public static implicit operator Material(MaterialRegistryRecord record)
        {
            return record.Value;
        }
    }
}