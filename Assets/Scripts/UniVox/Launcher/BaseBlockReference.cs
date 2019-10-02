using UnityEngine;
using UniVox.Managers.Game.Accessor;
using UniVox.Types;
using UniVox.Types.Identities;
using UniVox.Types.Keys;
using UniVox.VoxelData;

namespace UniVox.Launcher
{
    public abstract class BaseBlockReference
    {
        protected BlockKey BlockKey;
        protected IconKey IconKey;
        
        public abstract void RenderPass(BlockAccessor blockData);
        
        public abstract ArrayMaterialIdentity GetMaterial();
        public abstract FaceSubMaterial GetSubMaterial();
        public abstract Sprite GetBlockIcon();
        public abstract BlockIdentity GetBlockId();
    }
}