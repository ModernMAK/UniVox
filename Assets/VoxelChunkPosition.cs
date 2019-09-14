using Unity.Entities;
using Unity.Mathematics;

public struct VoxelChunkPosition : IComponentData
{
    public int3 Value;
}