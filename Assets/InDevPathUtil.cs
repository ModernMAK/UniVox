using System.IO;
using UnityEngine;

public static class InDevPathUtil
{
    public static string WorldDirectory => Path.Combine(Application.persistentDataPath, "World");
}