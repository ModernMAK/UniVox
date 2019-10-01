using System;
using UnityEngine;

namespace UniVox.Types
{
    [Serializable]
    public class NamedValue<T>
    {
        [SerializeField]
        public string Name;
        [SerializeField]
        public T Value;
    }
}