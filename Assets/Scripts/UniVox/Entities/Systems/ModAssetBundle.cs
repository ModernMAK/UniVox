using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniVox.Entities.Systems
{
    public class ModAssetBundle : IDisposable
    {
        public ModAssetBundle(AssetBundle assetBundle)
        {
            Handle = assetBundle;
        }

        private AssetBundle Handle { get; }

        public void Dispose()
        {
            Handle.Unload(false);
        }

        public T LoadAsset<T>(string name) where T : Object
        {
            return Handle.LoadAsset<T>(name);
        }

        public bool Contains(string name)
        {
            return Handle.Contains(name);
        }

        public T[] LoadAllAssets<T>(string name) where T : Object
        {
            return Handle.LoadAllAssets<T>();
        }


        public static implicit operator AssetBundle(ModAssetBundle assetBundle)
        {
            return assetBundle.Handle;
        }

        public static explicit operator ModAssetBundle(AssetBundle assetBundle)
        {
            return new ModAssetBundle(assetBundle);
        }
    }
}