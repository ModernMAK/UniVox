using UnityEngine;

namespace UniVox.Entities.Systems.Registry
{
    public class AtlasRegionRegistryRecord
    {
        public AtlasRegionRegistryRecord(Rect region)
        {
            Value = region;
        }

        public Rect Value { get; }

        public static implicit operator Rect(AtlasRegionRegistryRecord record)
        {
            return record.Value;
        }
    }
}