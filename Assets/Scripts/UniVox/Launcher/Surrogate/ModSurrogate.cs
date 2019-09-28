using System;
using UnityEngine;

namespace UniVox.Launcher.Surrogate
{
    [CreateAssetMenu(menuName = "Custom Assets/Mod Proxy")]
    [Serializable]
    public class ModSurrogate : ScriptableObject
    {
        [SerializeField] public ModRegistryRecordSurrogate Values;
    }
}