using ECS.Data.Voxel;
using ECS.Voxel;
using ECS.Voxel.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine;
using int3 = Unity.Mathematics.int3;


//public class MeshFixSystem : JobComponentSystem
//{
//    protected override void OnCreate()
//    {
//        base.OnCreate();
//        _entityQuery = GetEntityQuery(
//            ComponentType.ReadOnly<FaceVisibility>(),
//            ComponentType.ReadOnly<MeshData>(),
//            typeof(RenderMesh));
//        _manager = EntityManager;
//        _bufferGroup = World.Active.GetExistingSystem<BeginPresentationEntityCommandBufferSystem>();
//    }
//
//    private EntityManager _manager;
//    private EntityCommandBufferSystem _bufferGroup;
//    private EntityQuery _entityQuery;
//
////    [BurstCompile]
//    struct MeshFixJob : IJobChunk
//    {
//        public ArchetypeChunkEntityType EntityType;
//        public ArchetypeChunkComponentType<FaceVisibility> FaceVisibilityType;
//        [ReadOnly] public ArchetypeChunkSharedComponentType<MeshData> MeshDataType;
//        public ArchetypeChunkSharedComponentType<RenderMesh> RenderMeshType;
//        public EntityManager Manager;
//        public EntityCommandBuffer Buffer;
//
//        private RenderMesh CopyHelper(Mesh m, RenderMesh original)
//        {
//            return new RenderMesh()
//            {
//                castShadows = original.castShadows,
//                layer = original.layer,
//                material = original.material,
//                mesh = m,
//                receiveShadows = original.receiveShadows,
//                subMesh = original.subMesh
//            };
//        }
//
//        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//        {
//            chunk.GetSharedComponentIndex()
//            var entities = chunk.GetNativeArray(EntityType);
//            var visibility = chunk.GetNativeArray(FaceVisibilityType);
//            var meshData = chunk.GetSharedComponentData(MeshDataType, Manager);
//            var renderMesh = chunk.GetSharedComponentData(RenderMeshType, Manager);
//            var empty = CopyHelper(meshData.Empty, renderMesh);
//            var cube = CopyHelper(meshData.Cube, renderMesh);
//            RenderMesh desired;
//
//            for (var i = 0; i < chunk.Count; i++)
//            {
//                //None
//                if (visibility[i].value == 0)
//                {
//                    desired = empty;
//                    if (desired.Equals(renderMesh))
//                    {
//                        Buffer.SetSharedComponent(entities[i],);
//                        Buffer.
//                    }
//                }
//            }
//            
//            chunk.GetSharedComponentData(entities[0])
//
//            throw new System.NotImplementedException();
//        }
//    }
//
//    protected override JobHandle OnUpdate(JobHandle inputDependencies)
//    {
//        World.Active.CreateCommandBuffer();
//        var job = new MeshFixJob();
//        job.EntityType = GetArchetypeChunkEntityType();
//        
//        EntityManager.getsh
////        var worldPositions = _lookupQuery.ToComponentDataArray<WorldPosition>(Allocator.TempJob);
////        var voxelData = _lookupQuery.ToComponentDataArray<VoxelData>(Allocator.TempJob);
////        job.lookupWorldPositions = worldPositions;
////        job.lookupVoxelData = voxelData;
//        // Assign values to the fields on your job here, so that it has
//        // everything it needs to do its work when it runs later.
//        // For example,
//        //     job.deltaTime = UnityEngine.Time.deltaTime;
//
//
//        // Now that the job is set up, schedule it to be run. 
//        return job.Schedule(_entityQuery, inputDependencies);
//    }
//}

public class VisibilitySystem : JobComponentSystem
{
    private EntityQuery _lookupQuery;
//    private EntityQuery _workQuery;

    protected override void OnCreate()
    {
        _lookupQuery = GetEntityQuery(
            ComponentType.ReadOnly<WorldPosition>(),
            ComponentType.ReadOnly<VoxelData>());
    }
    [BurstCompile]
    struct VisibilitySystemJob : IJobForEach<WorldPosition, VoxelData, FaceVisibility>
    {
        
        public NativeArray<WorldPosition> lookupWorldPositions;
        public NativeArray<VoxelData> lookupVoxelData;


        private bool TryFindActive(int3 value, out bool result)
        {
            for (var i = 0; i < lookupWorldPositions.Length; i++)
            {
                if (!lookupWorldPositions[i].value.Equals(value)) continue;
                result = lookupVoxelData[i].Active;
                return true;
            }

            result = false;
            return false;
        }

        private static readonly int3 Up = new int3(0, 1, 0);
        private static readonly int3 Right = new int3(1, 0, 0);
        private static readonly int3 Forward = new int3(0, 0, 1);

        private void Helper(ref Directions flag, int3 position, int3 directionVector, Directions directionFlag)
        {

//            var temp = new Entity();
            if (TryFindActive(position + directionVector, out var result))
            {
                if (result)
                    flag |= Directions.Up;
                else
                    flag &= ~Directions.Up;
            }
        }

        public void Execute([ReadOnly] ref WorldPosition worldPosition, [ReadOnly] ref VoxelData voxelData,
            ref FaceVisibility faceVisibility)
        {
            var visibility = faceVisibility.value;
            var position = worldPosition.value;

            Helper(ref visibility, position, Up, Directions.Up);
            Helper(ref visibility, position, -Up, Directions.Down);
            Helper(ref visibility, position, Right, Directions.Right);
            Helper(ref visibility, position, -Right, Directions.Left);
            Helper(ref visibility, position, Forward, Directions.Forward);
            Helper(ref visibility, position, -Forward, Directions.Backward);

            faceVisibility.value = visibility;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new VisibilitySystemJob();
        var worldPositions = _lookupQuery.ToComponentDataArray<WorldPosition>(Allocator.TempJob);
        var voxelData = _lookupQuery.ToComponentDataArray<VoxelData>(Allocator.TempJob);
        job.lookupWorldPositions = worldPositions;
        job.lookupVoxelData = voxelData;
        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}