using ECS.Data.Voxel;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring
{
    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public class Universe : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddSharedComponentData(entity, new ChunkSize());
            dstManager.AddSharedComponentData(entity, new UniverseTable());
        }
    }
}