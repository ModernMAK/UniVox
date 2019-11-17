using UnityEngine;
using UniVox.Types;

namespace UniVox.Launcher
{
    /// <summary>
    /// Base Container for Blocks.
    /// </summary>
    /// See <see cref="NativeBlock"/> for the Native Variant.
    public abstract class AbstractBlock
    {
        /// <summary>
        /// Gets the Material Identity.
        /// </summary>
        /// <returns>This Block's Material Identity</returns>
        // TODO Due to the nature of Block's we can't guarantee they are cached. Later we can choose to enforce it, or realize we can't enforce it.
        public abstract MaterialIdentity GetMaterial();

        /// <summary>
        /// Gets the Sub Material.
        /// </summary>
        /// <returns>This Block's SubMaterial</returns>
        // TODO Due to the nature of Block's we can't guarantee they are cached. Later we can choose to enforce it, or realize we can't enforce it.
        public abstract FaceSubMaterial GetSubMaterial();

        public abstract Sprite GetBlockIcon();


        public NativeBlock GetNative()
        {
            return new NativeBlock(this);
        }
    }

    /// <summary>
    /// This is the Native Block Type.
    /// This struct cannot 
    /// This is a struct-only, simplified version of it's sister-type: <seealso cref="AbstractBlock"/>
    /// </summary>
    /// See <see cref="AbstractBlock"/> for the Non-Native Variant.
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