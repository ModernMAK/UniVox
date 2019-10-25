using UnityEngine;
using UniVox.Managers.Game.Accessor;
using UniVox.Types;

namespace UniVox.Launcher
{
    public abstract class BaseBlockReference
    {

        public abstract MaterialIdentity GetMaterial();
        public abstract FaceSubMaterial GetSubMaterial();

        public abstract Sprite GetBlockIcon();


        public NativeBaseBlockReference GetNative()
        {
            return new NativeBaseBlockReference(this);
        }
    }

    public struct NativeBaseBlockReference
    {
        public NativeBaseBlockReference(BaseBlockReference blockRef)
        {
            Material = blockRef.GetMaterial();
            SubMaterial = blockRef.GetSubMaterial();
        }

        public MaterialIdentity Material;
        public FaceSubMaterial SubMaterial;
    }
}