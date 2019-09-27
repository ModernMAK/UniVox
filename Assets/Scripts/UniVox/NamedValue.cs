using System;
using UnityEngine;

[Serializable]
public class NamedValue<T>
{
    [SerializeField]
    public string Name;
    [SerializeField]
    public T Value;
}