using System.IO;
using UnityEngine;

namespace UniVox.Asset_Management
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

        public static Mesh LoadMesh(string name, string modPath = default)
        {
            const string assetFolder = "Meshes";
            return Load<Mesh>(name, modPath, assetFolder);
        }

        public static Material LoadMaterial(string name, string modPath = default)
        {
            const string assetFolder = "Atlases";
            return Load<Material>(name, modPath, assetFolder);
        }


        public static Sprite LoadSprite(string name, string modPath = default)
        {
            const string assetFolder = "Sprites";
            return Load<Sprite>(name, modPath, assetFolder);
        }

        public static Texture LoadTexture(string name, string modPath = default)
        {
            const string assetFolder = "Textures";
            return Load<Texture>(name, modPath, assetFolder);
        }

        public static Texture LoadTexture2D(string name, string modPath = default)
        {
            const string assetFolder = "Textures";
            return Load<Texture2D>(name, modPath, assetFolder);
        }

        public static Texture LoadTexture3D(string name, string modPath = default)
        {
            const string assetFolder = "Textures";
            return Load<Texture3D>(name, modPath, assetFolder);
        }
    }
}