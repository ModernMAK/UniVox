using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniVox.Rendering.Render
{
    public struct ChunkRenderMesh : ISharedComponentData
    {
        // Oooh boy, SO. Heres the thing.
        // I respect that only SharedComponents can store References, it makes sense, since ECS makes you jump through hurdles to get arrays of them.
        // But, it makes our life harder since we know for a fact that each of our ChunkRenderMeshes are unique
        // For thoes unfamiliar, that means each EcsChunk has a single entity, and we cant iterate over each entity in a single chunk
        //Really just bloats our proccess step

        public Mesh Mesh;
        public Material Material;

        public ShadowCastingMode CastShadows;

        public bool ReceiveShadows;

        public int SubMesh;

        public int Layer;
    }
}