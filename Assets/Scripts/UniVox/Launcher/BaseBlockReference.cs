using UnityEngine;
using UniVox.Managers.Game.Accessor;
using UniVox.Types;
using UniVox.Types.Identities;
using UniVox.Types.Keys;

namespace UniVox.Launcher
{
    public abstract class BaseBlockReference
    {
        protected BlockKey BlockKey;
        protected IconKey IconKey;

        public abstract ArrayMaterialIdentity GetMaterial();
        public abstract FaceSubMaterial GetSubMaterial();

        public abstract Sprite GetBlockIcon();
        public abstract BlockIdentity GetBlockId();


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

        public ArrayMaterialIdentity Material;
        public FaceSubMaterial SubMaterial;
    }
}