using System;

[Serializable]
public class NamedValue<T>
{
    public string Name;
    public T Value;
}