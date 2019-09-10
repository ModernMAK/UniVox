using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class BaseGameMod : AbstractMod
{
    public const string ModPath = "BaseGame";

    public override void Initialize(ModInitializer initializer)
    {
        var meshReg = initializer.Registries.Mesh;

        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var mr = temp.GetComponent<MeshRenderer>();
//        var asset = ModResources.Load<Material>("Error",Path.Combine(Application.streamingAssetsPath, "Materials"));
        using (var asset = ModAssets.LoadMaterialBundle(Application.dataPath, ModPath))
        {
            var tempMat = asset.LoadAsset<Material>("ErrorMaterial");
            mr.material = new Material(tempMat);
        }

//        meshReg.LoadAndRegisterMesh("Block", "Cube", "BaseGame");
    }
}

public class ModAssetBundle : IDisposable
{
    public ModAssetBundle(AssetBundle assetBundle)
    {
        Handle = assetBundle;
    }

    private AssetBundle Handle { get; }

    public T LoadAsset<T>(string name) where T : Object => Handle.LoadAsset<T>(name);
    public bool Contains(string name) => Handle.Contains(name);
    public T[] LoadAllAssets<T>(string name) where T : Object => Handle.LoadAllAssets<T>();


    public static implicit operator AssetBundle(ModAssetBundle assetBundle)
    {
        return assetBundle.Handle;
    }

    public static explicit operator ModAssetBundle(AssetBundle assetBundle)
    {
        return new ModAssetBundle(assetBundle);
    }

    public void Dispose()
    {
        Handle.Unload(false);
    }
}