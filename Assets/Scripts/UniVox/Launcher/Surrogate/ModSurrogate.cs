using System;
using UnityEngine;

namespace UniVox.Entities.Systems.Surrogate
{
    [CreateAssetMenu(menuName = "Custom Assets/Mod Proxy")]
    [Serializable]
    public class ModSurrogate : ScriptableObject
    {
        [SerializeField] public ModRegistryRecordSurrogate Values;
    }
}