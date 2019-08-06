using System;
using Unity.Entities;
using Unity.Mathematics;

[Obsolete]
public struct SpawnUniverseEvent : IComponentData
{
    public Entity UniversePrefab;
    public int3 ChunkSize;
}