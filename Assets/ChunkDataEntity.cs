using ECS.Voxel.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = System.Random;

public class ChunkDataEntity
{
    public const int ChunkSizePerAxis = 32;
    public const int FlatSize = ChunkSizePerAxis * ChunkSizePerAxis * ChunkSizePerAxis;

    public ChunkData Chunk;
    public Entity[] EntityTable;

    public ChunkDataEntity() : this(new ChunkData())
    {
    }

    public ChunkDataEntity(ChunkData chunkData)
    {
        EntityTable = new Entity[FlatSize];
        Chunk = chunkData;
    }

    public void Init()
    {
        Init(Chunk);
        UpdateCulled();
    }

    public static void Init(ChunkData data)
    {
        SetupActive(data);
        SetupVisibility(data);
    }

    public static void SetupActive(ChunkData data)
    {
        var r = new Random();
        foreach (var pos in VoxelPos32.GetAllPositions())
            data.SolidTable[pos] = true;
    }

    public static void SetupVisibility(ChunkData data)
    {
        foreach (var pos in VoxelPos32.GetAllPositions())
        {
            var intPos = pos.Position;
            var flags = DirectionsX.NoneFlag;

            if (intPos.x != 0)
                flags |= Directions.Left;
            if (intPos.x != VoxelPos32.MaxValue)
                flags |= Directions.Right;

            if (intPos.y != 0)
                flags |= Directions.Down;
            if (intPos.y != VoxelPos32.MaxValue)
                flags |= Directions.Up;

            if (intPos.z != 0)
                flags |= Directions.Backward;
            if (intPos.z != VoxelPos32.MaxValue)
                flags |= Directions.Forward;

            data.HiddenFaces[pos] = flags;
        }
    }

    public void UpdateCulled()
    {
        foreach (var pos in VoxelPos32.GetAllPositions())
            UpdateCulled(pos);
    }

    public void UpdateCulled(VoxelPos32 voxelPos32)
    {
        var em = World.Active.EntityManager;
        var e = EntityTable[voxelPos32];
        if (Chunk.HiddenFaces[voxelPos32].IsAll())
        {
            if (!em.HasComponent(e, typeof(Disabled)))
                em.AddComponent(e, typeof(Disabled));
        }
        else
        {
            if (em.HasComponent(e, typeof(Disabled)))
                em.RemoveComponent(e, typeof(Disabled));
        }
    }

    public void SpawnEntities(GameObject prefab, int3 offset = default)
    {
        var entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, World.Active);
        var em = World.Active.EntityManager;
        var offsetShift = new float3(1 / 2f);
        foreach (var pos in VoxelPos32.GetAllPositions())
        {
            var spawned = em.Instantiate(entityPrefab);
            em.SetComponentData(spawned, new Translation {Value = offset + pos.Position + offsetShift});
            EntityTable[pos] = spawned;
        }

        em.DestroyEntity(entityPrefab);
    }
}