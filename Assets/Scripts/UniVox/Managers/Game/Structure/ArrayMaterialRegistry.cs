using UnityEngine;
using UniVox.Managers.Generic;

namespace UniVox.Managers.Game.Structure
{
    public class ArrayMaterialRegistry : NamedRegistry<ArrayMaterial>
    {
        //Helper Function
        public int Register(string name, Material material)
        {
            base.Register(name, new ArrayMaterial(material), out var reference);
            return reference;
        }
    }
}