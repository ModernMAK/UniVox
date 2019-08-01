//using ECS.Data.Voxel;
//using ECS.Voxel;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Mathematics;
//using UnityEngine;
//
//public static class VoxelSpawnNonJobified
//{
//    public static Entity SpawnUniverse(GameObject universeGO, World world)
//    {
//        var universePrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(universeGO, world);
//        var universe = world.EntityManager.Instantiate(universePrefab);
//        return universe;
//    }
//
//    public static Entity SpawnChunk(Entity universe, int3 chunkPos, int3 chunkSize, GameObject chunkGO,
//        GameObject voxelGO, World world)
//    {
//        var chunkPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(chunkGO, world);
//        var voxelPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(voxelGO, world);
//
//        var em = world.EntityManager;
//
//        var universeTable = em.GetSharedComponentData<OldUniverseTable>(universe).value;
//
//
//        var chunkTable = new NativeHashMap<int3, Entity>();
//
//        var chunk = em.Instantiate(chunkPrefab);
//
//        universeTable.TryAdd(chunkPos, chunk);
//        em.SetSharedComponentData(chunk, new InUniverse() {value = universe});
//        em.SetComponentData(chunk, new OldChunkPosition() {value = chunkPos});
//        em.SetSharedComponentData(chunk, new ChunkSize() {value = chunkSize});
//        em.SetSharedComponentData(chunk, new OldChunkTable() {value = chunkTable});
//
//        var voxelCount = chunkSize.x * chunkSize.y * chunkSize.z;
//        var voxels = new NativeArray<Entity>(voxelCount, Allocator.Temp);
//        em.Instantiate(voxelPrefab, voxels);
//
//        var counter = 0;
//        for (var x = 0; x < chunkSize.x; x++)
//        for (var y = 0; x < chunkSize.y; y++)
//        for (var z = 0; x < chunkSize.z; z++)
//        {
//            var voxel = voxels[counter];
//            var pos = new int3(x, y, z);
//            em.SetSharedComponentData(voxel, new InUniverse() {value = universe});
//            em.SetSharedComponentData(voxel, new InChunk() {value = chunk});
//            em.SetComponentData(voxel, new VoxelPosition() {value = pos});
//            em.SetSharedComponentData(voxel, new ChunkSize() {value = chunkSize});
//            em.SetSharedComponentData(voxel, new OldVoxelChunkPosition() {value = chunkPos});
//            chunkTable.TryAdd(pos, voxel);
//        }
//
//        em.DestroyEntity(voxelPrefab);
//        em.DestroyEntity(chunkPrefab);
//
//        return chunk;
//    }
//}