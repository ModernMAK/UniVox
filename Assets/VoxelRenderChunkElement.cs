using Unity.Entities;
using UnityEdits.Rendering;

//Because we cant reserve a full Chunk, we instead rserve a small faction
//I chose 64 since a Chunk using a byte index can only store a cube of 64 voxels
[InternalBufferCapacity(64)]
public struct VoxelRenderChunkElement : IBufferElementData
{
    // These implicit conversions are optional, but can help reduce typing.
    public static implicit operator VoxelRenderChunkData(VoxelRenderChunkElement element)
    {
        return element.Value;
    }

    public static implicit operator VoxelRenderChunkElement(VoxelRenderChunkData data)
    {
        return new VoxelRenderChunkElement {Value = data};
    }

    // Actual value each buffer element will store.

    public VoxelRenderChunkData Value;


}