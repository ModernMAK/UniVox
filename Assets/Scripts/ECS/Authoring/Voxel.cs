﻿using ECS.Data.Voxel;
using ECS.Voxel;
using Unity.Entities;
using UnityEngine;

namespace ECS.Authoring
{
    [DisallowMultipleComponent]
    [RequiresEntityConversion]
    public class Voxel : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField] public MaterialList MaterialList;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //VOXEL DATA
            dstManager.AddComponentData(entity, new WorldPosition());
            dstManager.AddComponentData(entity, new VoxelPosition());

            //CHUNK DATA
            dstManager.AddSharedComponentData(entity, new VoxelChunkPosition());
            dstManager.AddSharedComponentData(entity, new ChunkSize());

            dstManager.AddSharedComponentData(entity, new VoxelMaterials() {Materials = MaterialList});
            dstManager.AddComponentData(entity, new VoxelRenderData(){MaterialIndex = 0});

            
            
//            dstManager.AddSharedComponentData(entity, new InChunk());
//            dstManager.AddSharedComponentData(entity, new InUniverse());
        }
    }
    
}