using System.IO;
using UnityEngine;

namespace UniVox.Entities.Systems
{
    public static class ModAssets
    {
        private static ModAssetBundle LoadBundle(string workingDirectory, string assetBundlePath)
        {
            return new ModAssetBundle(
                AssetBundle.LoadFromFile(Path.Combine(workingDirectory, assetBundlePath.ToLowerInvariant())));
        }

        private static ModAssetBundle LoadBundle(string workingDirectory, string bundleName, string modPath)
        {
            return LoadBundle(workingDirectory, Path.Combine(modPath, bundleName));
        }

        public static ModAssetBundle LoadModBundle(string workingDirectory, string modPath)
        {
            return LoadBundle(workingDirectory, modPath);
        }

        public static ModAssetBundle LoadMeshBundle(string workingDirectory, string modPath = default)
        {
            const string assetFolder = "meshes";
            return LoadBundle(workingDirectory, assetFolder, modPath);
        }

        public static ModAssetBundle LoadMaterialBundle(string workingDirectory, string modPath = default)
        {
            const string assetFolder = "materials";
            return LoadBundle(workingDirectory, assetFolder, modPath);
        }


        public static ModAssetBundle LoadSpriteBundle(string workingDirectory, string modPath = default)
        {
            const string assetFolder = "sprites";
            return LoadBundle(workingDirectory, assetFolder, modPath);
        }

        public static ModAssetBundle LoadTextureBundle(string workingDirectory, string modPath = default)
        {
            const string assetFolder = "textures";
            return LoadBundle(workingDirectory, assetFolder, modPath);
        }
    }
}