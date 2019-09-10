using System.IO;
using InventorySystem;
using UnityEngine;

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
        const string assetFolder = "Materials";
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

public static class ModAssets
{
    private static ModAssetBundle LoadBundle(string workingDirectory, string assetBundlePath)
    {
        return new ModAssetBundle(
            AssetBundle.LoadFromFile(Path.Combine(workingDirectory, assetBundlePath.ToLowerInvariant())));
    }

    private static ModAssetBundle LoadBundle(string workingDirectory, string bundleName, string modPath = default)
    {
        return LoadBundle(workingDirectory,Path.Combine(modPath, bundleName));
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