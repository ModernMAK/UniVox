using System;
using UnityEngine;
using UniVox.Managers.Game.Structure;

namespace UniVox.Launcher.Surrogate
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