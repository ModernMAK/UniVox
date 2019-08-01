using Unity.Entities;
using Unity.Mathematics;

public struct SpawnVoxelEvent : IComponentData
{
    public Entity Universe;
    public Entity Chunk;
    public Entity VoxelPrefab;
    public int3 VoxelPosition;
    public int3 ChunkPosition;
    public int3 ChunkSize;
}