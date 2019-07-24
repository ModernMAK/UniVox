using System.Collections;
using System.Collections.Generic;
using ECS.Voxel;
using ECS.Voxel.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class Chunk : MonoBehaviour
{
    public int3 ChunkSize;
    public GameObject VoxelPrefab;

    void SpawnChunk(GameObject voxelPrefab, int3 size, int3 position, World world)
    {
        var manager = world.EntityManager;
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(voxelPrefab, world);

//        var table = new Entity[size.x, size.y, size.z];
//        var chunkTableData = new ChunkTable() {value = table};
        var flatSize = size.x * size.y * size.z;
        var table = new NativeArray<Entity>(flatSize, Allocator.Persistent);

        var chunkPosData = new ChunkPosition() {value = position};
        var chunkTableData = new ChunkTableNative() {value = table};

        for (var x = 0; x < size.x; x++)
        for (var y = 0; y < size.y; y++)
        for (var z = 0; z < size.z; z++)
        {
            var spawnedEntity = manager.Instantiate(prefab);
            var localPosition = new int3(x, y, z);
            var worldPosition = position * size + localPosition;
            var index = ChunkTableNative.CalculateIndex(localPosition, size);
            table[index] = spawnedEntity;
            manager.SetSharedComponentData(spawnedEntity, chunkPosData);
            manager.SetSharedComponentData(spawnedEntity, chunkTableData);
            manager.SetComponentData(spawnedEntity, new LocalPosition() {value = localPosition});
//            manager.SetComponentData(spawnedEntity, new WorldPosition() {value = worldPosition});
//            manager.SetComponentData(spawnedEntity, new LocalP);
        }

        manager.DestroyEntity(prefab);
    }

    void SpawnChunk(int3 position)
    {
        SpawnChunk(VoxelPrefab, ChunkSize, position, World.Active);
    }

    void Start()
    {
        SpawnChunk(int3.zero);
    }
}