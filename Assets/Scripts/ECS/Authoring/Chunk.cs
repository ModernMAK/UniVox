using ECS.Data.Voxel;
using ECS.Voxel;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring
{
    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public class Chunk : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new WorldPosition());
            dstManager.AddComponentData(entity, new ChunkPosition());
            dstManager.AddSharedComponentData(entity, new ChunkSize());
            dstManager.AddSharedComponentData(entity, new ChunkTable());
//            dstManager.AddSharedComponentData(entity, new InUniverse());
        }
    }
}