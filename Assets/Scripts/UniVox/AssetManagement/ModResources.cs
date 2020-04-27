using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniVox.AssetManagement
{
    public static class ModResources
    {
        private static T Load<T>(string name, string modPath, string folderPath) where T : Object
        {
            return Resources.Load<T>(Path.Combine(modPath, folderPath, name));
        }

        public static T Load<T>(string name, string fullResourcePath) where T : Object
        {
            return Resources.Load<T>(Path.Combine(fullResourcePath, name));
        }

        [Obsolete]
        public static Mesh LoadMesh(string name, string modPath = default)
        {
            const string assetFolder = "Meshes";
            return Load<Mesh>(name, modPath, assetFolder);
        }

        [Obsolete]
        public static Material LoadMaterial(string name, string modPath = default)
        {
            const string assetFolder = "Atlases";
            return Load<Material>(name, modPath, assetFolder);
        }


        [Obsolete]
        public static Sprite LoadSprite(string name, string modPath = default)
        {
            const string assetFolder = "Sprites";
            return Load<Sprite>(name, modPath, assetFolder);
        }

        [Obsolete]
        public static Texture LoadTexture(string name, string modPath = default)
        {
            const string assetFolder = "Textures";
            return Load<Texture>(name, modPath, assetFolder);
        }

        [Obsolete]
        public static Texture LoadTexture2D(string name, string modPath = default)
        {
            const string assetFolder = "Textures";
            return Load<Texture2D>(name, modPath, assetFolder);
        }

        [Obsolete]
        public static Texture LoadTexture3D(string name, string modPath = default)
        {
            const string assetFolder = "Textures";
            return Load<Texture3D>(name, modPath, assetFolder);
        }
    }
}