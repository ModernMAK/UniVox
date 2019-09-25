using UnityEngine;

namespace UniVox.Entities.Systems.Surrogate
{
    [CreateAssetMenu(menuName = "Custom Assets/Mod Proxy")]
    public class ModSurrogate : ScriptableObject
    {
        public ModRegistryRecordSurrogate Values;
    }
}