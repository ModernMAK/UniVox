using System.Collections;
using ECS.Data.Voxel;
using ECS.Voxel;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

[DisallowMultipleComponent]
public class ChunkSpawner : MonoBehaviour
{
    public int3 ChunkSize;
    public int InnerLoop;
    public int3 UniverseSize;
    public GameObject VoxelPrefab;

    private void SpawnChunk(GameObject voxelPrefab, int3 size, int3 position, World world)
    {
        var manager = world.EntityManager;
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(voxelPrefab, world);


        for (var x = 0; x < size.x; x++)
        for (var y = 0; y < size.y; y++)
        for (var z = 0; z < size.z; z++)
        {
            var spawnedEntity = manager.Instantiate(prefab);
            var localPosition = new int3(x, y, z);
            manager.SetSharedComponentData(spawnedEntity, new VoxelChunkPosition {value = position});
            manager.SetSharedComponentData(spawnedEntity, new ChunkSize(size));
            manager.SetComponentData(spawnedEntity, new VoxelPosition {value = localPosition});
        }

        manager.DestroyEntity(prefab);
    }

    private void SpawnVoxel(Entity prefab, int3 chunkPos, int3 localPos, int3 size)
    {
    }

    private IEnumerator SpawnChunkAsync(GameObject voxelPrefab, int3 size, int3 position, World world,
        int innerLoop = byte.MaxValue)
    {
        if (innerLoop < 1)
            innerLoop = 1;

        var manager = world.EntityManager;
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(voxelPrefab, world);

        var matSize = manager.GetSharedComponentData<VoxelMaterials>(prefab).Materials.Count;

        var counter = 0;
        for (var x = 0; x < size.x; x++)
        for (var y = 0; y < size.y; y++)
        for (var z = 0; z < size.z; z++)
        {
            var spawnedEntity = manager.Instantiate(prefab);
            var localPosition = new int3(x, y, z);
            var worldPos = localPosition + ChunkSize * position;
            manager.SetSharedComponentData(spawnedEntity, new VoxelChunkPosition {value = position});
            manager.SetSharedComponentData(spawnedEntity, new ChunkSize(size));
            manager.SetComponentData(spawnedEntity, new VoxelPosition {value = localPosition});
            var rand = new Random(worldPos.GetHashCode());
            manager.SetComponentData(spawnedEntity, new VoxelRenderData {MaterialIndex = rand.Next(matSize)});

            if (counter > innerLoop)
            {
                counter = 0;
                yield return null;
            }
            else
            {
                counter++;
            }
        }

        manager.DestroyEntity(prefab);
    }

    private void SpawnChunk(int3 position)
    {
        SpawnChunk(VoxelPrefab, ChunkSize, position, World.Active);
    }

    private IEnumerator SpawnChunkAsync(int3 position, int innerLoop = byte.MaxValue)
    {
        yield return StartCoroutine(SpawnChunkAsync(VoxelPrefab, ChunkSize, position, World.Active, innerLoop));
    }

    private void SpawnWorld(int3 size)
    {
        for (var x = -size.x; x <= size.x; x++)
        for (var y = -size.y; y <= size.y; y++)
        for (var z = -size.z; z <= size.z; z++)
        {
            var pos = new int3(x, y, z);
            SpawnChunk(pos);
        }
    }

    private IEnumerator SpawnWorldAsync(int3 size, int innerLoop)
    {
        for (var x = -size.x; x <= size.x; x++)
        for (var y = -size.y; y <= size.y; y++)
        for (var z = -size.z; z <= size.z; z++)
        {
            var pos = new int3(x, y, z);
            yield return StartCoroutine(SpawnChunkAsync(pos, innerLoop));
        }
    }


    private void Start()
    {
        StartCoroutine(SpawnWorldAsync(UniverseSize, InnerLoop));
    }
}