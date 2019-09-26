using System;
using UnityEngine;
using UniVox.Entities.Systems.Registry;

namespace UniVox.Entities.Systems.Surrogate
{
    [Serializable]
    public class NamedAtlasRegion : NamedValue<Rect>
    {
        
    }
    [Serializable]
    public class NamedSubMaterial : NamedValue<int>
    {
        
    }
    [Serializable]
    public class NamedMesh : NamedValue<Mesh>
    {
        
    }
    [Serializable]
    public class NamedBlock : NamedValue<BaseBlockReference>
    {
        
    }
    [Serializable]
    public class NamedEntity : NamedValue<EntityRegistryRecord>
    {
        
    }
}