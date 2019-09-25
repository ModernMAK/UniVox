using UnityEngine;
using UniVox.Entities.Systems.Registry;

namespace UniVox.Entities.Systems.Surrogate
{
    [CreateAssetMenu(menuName = "Custom Assets/Mod List Proxy")]
    public class ModRegistrySurrogate : ScriptableObject
    {
        public ModRegistryRecordSurrogate[] Values;
    }
}