using System.Collections;
using System.Collections.Generic;
using ECS.Voxel;
using ECS.Voxel.Data;
using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class Voxel : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new WorldPosition());
        dstManager.AddComponentData(entity, new LocalPosition());
        dstManager.AddSharedComponentData(entity, new ChunkSize());
        dstManager.AddSharedComponentData(entity, new ChunkTableNative());
//        dstManager.AddSharedComponentData(entity, new ChunkTable());
        dstManager.AddSharedComponentData(entity, new ChunkPosition());
    }
}