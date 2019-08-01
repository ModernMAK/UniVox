using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


[RequiresEntityConversion]
public class UniverseSpawner : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public GameObject _UniversePrefab;
    public GameObject _ChunkPrefab;
    public GameObject _VoxelPrefab;
    public int3 ChunkSize;

    // Referenced prefabs have to be declared so that the conversion system knows about them ahead of time
    public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
    {
        gameObjects.Add(_UniversePrefab);
        gameObjects.Add(_ChunkPrefab);
        gameObjects.Add(_VoxelPrefab);
    }

    // Lets you convert the editor data representation to the entity optimal runtime representation

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var spawnerData = new SpawnUniverseEvent()
        {
            // The referenced prefab will be converted due to DeclareReferencedPrefabs.
            // So here we simply map the game object to an entity reference to that prefab.
            UniversePrefab = conversionSystem.GetPrimaryEntity(_UniversePrefab),
            ChunkSize = ChunkSize
        };
        dstManager.AddComponentData(entity, spawnerData);


        var chunk = dstManager.CreateEntity(typeof(SpawnChunkEvent));
        dstManager.SetComponentData(chunk,new SpawnChunkEvent()
        {
            ChunkPosition = int3.zero,
            ChunkPrefab = conversionSystem.GetPrimaryEntity(_ChunkPrefab),
            ChunkSize = ChunkSize,
        });
        
        

        var voxel = dstManager.CreateEntity(typeof(SpawnChunkEvent));
        dstManager.SetComponentData(chunk,new SpawnChunkEvent()
        {
            ChunkPosition = int3.zero,
            ChunkPrefab = conversionSystem.GetPrimaryEntity(_ChunkPrefab),
            ChunkSize = ChunkSize,
        });
        
    }
}