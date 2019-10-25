using UnityEngine;
using UniVox.Managers.Game.Accessor;
using UniVox.Types;

namespace UniVox.Launcher
{
    public abstract class AbstractBlock
    {

        public abstract MaterialIdentity GetMaterial();
        public abstract FaceSubMaterial GetSubMaterial();

        public abstract Sprite GetBlockIcon();


        public NativeBlock GetNative()
        {
            return new NativeBlock(this);
        }
    }

    public struct NativeBlock
    {
        public NativeBlock(AbstractBlock blockRef)
        {
            Material = blockRef.GetMaterial();
            SubMaterial = blockRef.GetSubMaterial();
        }

        public MaterialIdentity Material;
        public FaceSubMaterial SubMaterial;
    }
}