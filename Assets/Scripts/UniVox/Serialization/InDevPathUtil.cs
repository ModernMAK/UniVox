using System.IO;
using UnityEngine;

namespace UniVox.Serialization
{
    public static class InDevPathUtil
    {
        
        public static string SaveDirectory => Path.Combine(Application.persistentDataPath, "Saves");
    }
}