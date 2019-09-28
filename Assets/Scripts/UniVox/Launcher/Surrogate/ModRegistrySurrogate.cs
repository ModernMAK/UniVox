using UnityEngine;

namespace UniVox.Launcher.Surrogate
{
    [CreateAssetMenu(menuName = "Custom Assets/Mod List Proxy")]
    public class ModRegistrySurrogate : ScriptableObject
    {
        public ModRegistryRecordSurrogate[] Values;
    }
}