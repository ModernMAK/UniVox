using Unity.Entities;
using Unity.Mathematics;

public struct SpawnUniverseEvent : IComponentData
{
    public Entity UniversePrefab;
    public int3 ChunkSize;
}