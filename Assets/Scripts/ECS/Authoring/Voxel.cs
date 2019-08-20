using ECS.Data.Voxel;
using ECS.Voxel;
using ECS.Voxel.Data;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring
{
    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public class Voxel : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] public MaterialList MaterialList;
        [SerializeField] public MeshList MeshList;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //VOXEL DATA
            dstManager.AddComponentData(entity, new WorldPosition());
            dstManager.AddComponentData(entity, new VoxelPosition());

            //CHUNK DATA
            dstManager.AddSharedComponentData(entity, new VoxelChunkPosition());
            dstManager.AddSharedComponentData(entity, new ChunkSize());


            dstManager.AddSharedComponentData(entity, new VoxelMaterials {Materials = MaterialList});

            if (MeshList != null)
                dstManager.AddSharedComponentData(entity, new VoxelShapes {Lookup = MeshList.CreateDictionary()});
            dstManager.AddComponentData(entity, new VoxelRenderData {MaterialIndex = 0, MeshShape = BlockShape.Cube});


//            dstManager.AddSharedComponentData(entity, new InChunk());
//            dstManager.AddSharedComponentData(entity, new InUniverse());
        }
    }
}