using System.IO;
using UnityEngine;

namespace UniVox.Serialization
{
    public static class InDevPathUtil
    {
        public static string WorldDirectory => Path.Combine(Application.persistentDataPath, "World");
    }
}