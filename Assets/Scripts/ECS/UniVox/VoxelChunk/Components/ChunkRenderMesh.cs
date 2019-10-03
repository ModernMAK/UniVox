using System;
using Unity.Entities;
using UnityEngine.Rendering;
using ECS.UniVox.VoxelChunk.Systems;

namespace ECS.UniVox.VoxelChunk.Components
{
    [Serializable]
    public struct ChunkRenderMesh : IComponentData
    {
        /// <summary>
        /// The value used to lookup the mesh in the ChunkRender System;
        /// <see cref="ChunkRenderMeshSystem"/> for more info.
        /// </summary>
        public BatchGroupIdentity Batch;

        /// <summary>
        /// Shadow casting settings. <see cref="ShadowCastingMode"/>
        /// </summary>
        public ShadowCastingMode CastShadows;

        /// <summary>
        /// Whether the mesh should receive shadows
        /// </summary>
        public bool ReceiveShadows;


        //TODO this is always 0; safely remove it
        public int SubMesh;

        /// <summary>
        /// The Layer to render to? TODO remember what this does again
        /// </summary>
        public int Layer;
    }
}