using UnityEngine;

namespace UniVox.Entities.Systems.Surrogate
{
    public class MaterialRegistryProxyRecord
    {
        
        public Material Value { get; }
        public AtlasRegionRegistryRecordSurrogate[] Regions { get; }

    }
}