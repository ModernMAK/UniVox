using System;
using UnityEngine;
using Object = UnityEngine.Object;

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