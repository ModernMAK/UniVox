//using System.Collections;
//using System.Collections.Generic;
//using ECS.UniVox.Systems;
//using ECS.UniVox.VoxelChunk.Systems;
//using Unity.Entities;
//using Unity.Mathematics;
//using Unity.Transforms;
//using UnityEngine;
//
//public class CameraEntityProxy : MonoBehaviour
//{
//    private EntityManager _em;
//
//    private Entity _cameraEntity;
//
//
//    [SerializeField] private int3 _size = new int3(1, 1, 1);
//
//
//    [SerializeField] private int _framesPerUpdate = 60;
//
//    private int _frames;
//
//    // Start is called before the first frame update
//    void Start()
//    {
//        _em = World.Active.EntityManager;
//        _cameraEntity = _em.CreateEntity(
//            ComponentType.ReadWrite<LocalToWorld>(),
//            ComponentType.ReadWrite<ChunkStreamingTarget>()
//        );
//    }
//
//    // Update is called once per frame
//    void Update()
//    {
//        if (_framesPerUpdate < _frames)
//        {
//            UpdateEntity();
//            _frames = 0;
//        }
//        else
//        {
//            _frames++;
//        }
//    }
//
//    void UpdateEntity()
//    {
//        _em.SetComponentData(_cameraEntity, new LocalToWorld()
//        {
//            Value = transform.localToWorldMatrix
//        });
//
//        _em.SetComponentData(_cameraEntity, new ChunkStreamingTarget()
//        {
//            Distance = _size
//        });
//    }
//}