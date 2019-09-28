using UnityEngine;
using UniVox.Managers.Generic;

namespace UniVox.Managers.Game.Structure
{
    public class AtlasRegistry : NamedRegistry<AtlasMaterial>
    {
        //Helper Function
        public int Register(string name, Material material)
        {
            base.Register(name, new AtlasMaterial(material), out var reference);
            return reference;
        }
    }
}