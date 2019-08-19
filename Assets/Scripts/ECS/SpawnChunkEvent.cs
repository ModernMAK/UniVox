using System;
using Unity.Entities;
using Unity.Mathematics;

[Obsolete]
public struct SpawnChunkEvent : IComponentData
{
    public Entity Universe;
    public Entity ChunkPrefab;
    public int3 ChunkPosition;
    public int3 ChunkSize;
}