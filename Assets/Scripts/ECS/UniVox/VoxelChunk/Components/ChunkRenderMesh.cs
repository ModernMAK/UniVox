using System;
using Unity.Entities;
using UnityEngine.Rendering;

namespace ECS.UniVox.VoxelChunk.Components
{
    [Serializable]
    public struct ChunkRenderMesh : IComponentData
    {
        // Oooh boy, SO. Heres the thing.
        // I respect that only SharedComponents can store References, it makes sense, since ECS makes you jump through hurdles to get arrays of them.
        // But, it makes our life harder since we know for a fact that each of our ChunkRenderMeshes are unique
        // For thoes unfamiliar, that means each EcsChunk has a single entity, and we cant iterate over each entity in a single chunk
        //Really just bloats our proccess step
        //WHICH IS WHy
        //I use a BatchIDentity
        //IT couples the GEneration System with the Render System
        //But we dont have to deal with the Bloat of using references in ComponentData


        public BatchGroupIdentity Batch;

        public ShadowCastingMode CastShadows;

        public bool ReceiveShadows;

        public int SubMesh;

        public int Layer;
    }

    // If this is present, there should be at least one mesh, so we tell the buffer to start with 1
    [InternalBufferCapacity(1)]
    public struct ChunkMeshBuffer : IBufferElementData
    {
        // Oooh boy, SO. Heres the thing.
        // I respect that only SharedComponents can store References, it makes sense, since ECS makes you jump through hurdles to get arrays of them.
        // But, it makes our life harder since we know for a fact that each of our ChunkRenderMeshes are unique
        // For thoes unfamiliar, that means each EcsChunk has a single entity, and we cant iterate over each entity in a single chunk
        //Really just bloats our proccess step
        //WHICH IS WHy
        //I use a BatchIDentity
        //IT couples the GEneration System with the Render System
        //But we dont have to deal with the Bloat of using references in ComponentData


        public BatchGroupIdentity Batch;

        public ShadowCastingMode CastShadows;

        public bool ReceiveShadows;

        public int SubMesh;

        public int Layer;
    }
}