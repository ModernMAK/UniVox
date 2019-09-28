using System;
using UnityEngine;

namespace UniVox
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