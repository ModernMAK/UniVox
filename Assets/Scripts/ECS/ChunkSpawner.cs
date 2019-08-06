using System.Collections;
using System.Collections.Generic;
using ECS.Data.Voxel;
using ECS.Voxel;
using ECS.Voxel.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class ChunkSpawner : MonoBehaviour
{
    public int3 ChunkSize;
    public int3 UniverseSize;
    public GameObject VoxelPrefab;
    public int InnerLoop;

    void SpawnChunk(GameObject voxelPrefab, int3 size, int3 position, World world)
    {
        var manager = world.EntityManager;
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(voxelPrefab, world);

//        var table = new Entity[size.x, size.y, size.z];
//        var chunkTableData = new ChunkTable() {value = table};
//        var flatSize = size.x * size.y * size.z;
//        var table = new NativeHashMap<int3, Entity>(flatSize, Allocator.Persistent);

//        var chunkPosData = new VoxelChunkPosition() {value = position};
//        var chunkTableData = new OldChunkTable() {value = table};

        for (var x = 0; x < size.x; x++)
        for (var y = 0; y < size.y; y++)
        for (var z = 0; z < size.z; z++)
        {
            var spawnedEntity = manager.Instantiate(prefab);
            var localPosition = new int3(x, y, z);
//            var worldPosition = position * size + localPosition;
//            var index = ChunkTable.CalculateIndex(localPosition, size);
//            table.TryAdd(localPosition, spawnedEntity);
//            manager.SetSharedComponentData(spawnedEntity, chunkPosData);
//            manager.SetSharedComponentData(spawnedEntity, chunkTableData);
            manager.SetSharedComponentData(spawnedEntity, new VoxelChunkPosition() {value = position});
            manager.SetSharedComponentData(spawnedEntity, new ChunkSize(size));
            manager.SetComponentData(spawnedEntity, new VoxelPosition() {value = localPosition});
//            manager.SetComponentData(spawnedEntity, new WorldPosition() {value = worldPosition});
//            manager.SetComponentData(spawnedEntity, new LocalP);
        }

        manager.DestroyEntity(prefab);
    }

    void SpawnVoxel(Entity prefab, int3 chunkPos, int3 localPos, int3 size)
    {
        
    }
    
    IEnumerator SpawnChunkAsync(GameObject voxelPrefab, int3 size, int3 position, World world,
        int innerLoop = byte.MaxValue)
    {
        if (innerLoop < 1)
            innerLoop = 1;

        var manager = world.EntityManager;
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(voxelPrefab, world);

//        var table = new Entity[size.x, size.y, size.z];
//        var chunkTableData = new ChunkTable() {value = table};
//        var flatSize = size.x * size.y * size.z;
//        var table = new NativeHashMap<int3, Entity>(flatSize, Allocator.Persistent);

//        var chunkPosData = new VoxelChunkPosition() {value = position};
//        var chunkTableData = new OldChunkTable() {value = table};

        var counter = 0;
        for (var x = 0; x < size.x; x++)
        for (var y = 0; y < size.y; y++)
        for (var z = 0; z < size.z; z++)
        {
            var spawnedEntity = manager.Instantiate(prefab);
            var localPosition = new int3(x, y, z);
//            var worldPosition = position * size + localPosition;
//            var index = ChunkTable.CalculateIndex(localPosition, size);
//            table.TryAdd(localPosition, spawnedEntity);
//            manager.SetSharedComponentData(spawnedEntity, chunkPosData);
//            manager.SetSharedComponentData(spawnedEntity, chunkTableData);
            manager.SetSharedComponentData(spawnedEntity, new VoxelChunkPosition() {value = position});
            manager.SetSharedComponentData(spawnedEntity, new ChunkSize(size));
            manager.SetComponentData(spawnedEntity, new VoxelPosition() {value = localPosition});
//            manager.SetComponentData(spawnedEntity, new WorldPosition() {value = worldPosition});
//            manager.SetComponentData(spawnedEntity, new LocalP);

            if (counter > innerLoop)
            {
                counter = 0;
                yield return null;
            }
            else counter++;
        }

        manager.DestroyEntity(prefab);
    }

    void SpawnChunk(int3 position)
    {
        SpawnChunk(VoxelPrefab, ChunkSize, position, World.Active);
    }

    IEnumerator SpawnChunkAsync(int3 position, int innerLoop = byte.MaxValue)
    {
        yield return StartCoroutine(SpawnChunkAsync(VoxelPrefab, ChunkSize, position, World.Active, innerLoop));
    }

    void SpawnWorld(int3 size)
    {
        for (var x = -size.x; x <= size.x; x++)
        for (var y = -size.y; y <= size.y; y++)
        for (var z = -size.z; z <= size.z; z++)
        {
            var pos = new int3(x, y, z);
            SpawnChunk(pos);
        }
    }

    IEnumerator SpawnWorldAsync(int3 size, int innerLoop)
    {
        for (var x = -size.x; x <= size.x; x++)
        for (var y = -size.y; y <= size.y; y++)
        for (var z = -size.z; z <= size.z; z++)
        {
            var pos = new int3(x, y, z);
            yield return StartCoroutine(SpawnChunkAsync(pos, innerLoop));
        }
    }


    void Start()
    {
//        SpawnChunk(int3.zero);

        StartCoroutine(SpawnWorldAsync(UniverseSize, InnerLoop));
    }
}