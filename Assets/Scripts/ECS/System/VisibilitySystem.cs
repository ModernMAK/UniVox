//using ECS.Voxel;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Rendering;
//using UnityEngine;
//using int3 = Unity.Mathematics.int3;
//
//
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
//        _bufferGroup = World.Active.GetExistingSystem<BeginPresentationEntityCommandBufferSystem>()
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
//
//public class VisibilitySystem : JobComponentSystem
//{
//    private EntityQuery _lookupQuery;
////    private EntityQuery _workQuery;
//
//    protected override void OnCreate()
//    {
//        _lookupQuery = GetEntityQuery(ComponentType.ReadOnly<WorldPosition>(),
//            ComponentType.ReadOnly<VoxelData>());
////        _workQuery = GetEntityQuery(ComponentType.ReadOnly<WorldPosition>(),
////            ComponentType.ReadOnly<lookupVoxelData>());
//    }
//
//    // This declares a new kind of job, which is a unit of work to do.
//    // The job is declared as an IJobForEach<Translation, Rotation>,
//    // meaning it will process all entities in the world that have both
//    // Translation and Rotation components. Change it to process the component
//    // types you want.
//    //
//    // The job is also tagged with the BurstCompile attribute, which means
//    // that the Burst compiler will optimize it for the best performance.
//    [BurstCompile]
//    struct VisibilitySystemJob : IJobForEach<WorldPosition, VoxelData, FaceVisibility>
//    {
//        // Add fields here that your job needs to do its work.
//        // For example,
//        //    public float deltaTime;
//
//
////        public void Execute(ref Translation translation, [ReadOnly] ref Rotation rotation)
//        //        {
//        //            // Implement the work to perform for each entity here.
//        //            // You should only access data that is local or that is a
//        //            // field on this job. Note that the 'rotation' parameter is
//        //            // marked as [ReadOnly], which means it cannot be modified,
//        //            // but allows this job to run in parallel with other jobs
//        //            // that want to read Rotation component data.
//        //            // For example,
//        //            //     translation.Value += mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;
//        //            
//        //            
//        //        }
//        //
//        //        public void Execute(ref WorldPosition position, ref lookupVoxelData data, ref FaceVisibility visibility)
//        //        {
//        //            throw new System.NotImplementedException();
//        //        }
////        public EntityQuery _lookupQuery;
////            [ ReadOnly] public ArchetypeChunkComponentType<WorldPosition> WorldPositionType;
////        [ReadOnly] public ArchetypeChunkComponentType<lookupVoxelData> VoxelDataType;
////        public ArchetypeChunkComponentType<FaceVisibility> FaceVisibilityType;
////
////        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
////        {
////            _lookupQuery.
////            var worldPosition = chunk.GetNativeArray(WorldPositionType);
////            var lookupVoxelData = chunk.GetNativeArray(VoxelDataType);
////            var faceVisibility = chunk.GetNativeArray(FaceVisibilityType);
////
////            throw new System.NotImplementedException();
////        }
//        public NativeArray<WorldPosition> lookupWorldPositions;
//        public NativeArray<VoxelData> lookupVoxelData;
//
//
//        private bool TryFindActive(int3 value, out bool result)
//        {
//            for (var i = 0; i < lookupWorldPositions.Length; i++)
//            {
//                if (!lookupWorldPositions[i].value.Equals(value)) continue;
//                result = lookupVoxelData[i].Active;
//                return true;
//            }
//
//            result = false;
//            return false;
//        }
//
//        private static readonly int3 Up = new int3(0, 1, 0);
//        private static readonly int3 Right = new int3(1, 0, 0);
//        private static readonly int3 Forward = new int3(0, 0, 1);
//
//        private void Helper(ref Directions flag, int3 position, int3 directionVector, Directions directionFlag)
//        {
//            if (TryFindActive(position + directionVector, out var result))
//            {
//                if (result)
//                    flag |= Directions.Up;
//                else
//                    flag &= ~Directions.Up;
//            }
//        }
//
//        public void Execute([ReadOnly] ref WorldPosition worldPosition, [ReadOnly] ref VoxelData voxelData,
//            ref FaceVisibility faceVisibility)
//        {
//            var visibility = faceVisibility.value;
//            var position = worldPosition.value;
//
//            Helper(ref visibility, position, Up, Directions.Up);
//            Helper(ref visibility, position, -Up, Directions.Down);
//            Helper(ref visibility, position, Right, Directions.Right);
//            Helper(ref visibility, position, -Right, Directions.Left);
//            Helper(ref visibility, position, Forward, Directions.Forward);
//            Helper(ref visibility, position, -Forward, Directions.Backward);
//
//            faceVisibility.value = visibility;
//        }
//    }
//
//    protected override JobHandle OnUpdate(JobHandle inputDependencies)
//    {
//        var job = new VisibilitySystemJob();
//        var worldPositions = _lookupQuery.ToComponentDataArray<WorldPosition>(Allocator.TempJob);
//        var voxelData = _lookupQuery.ToComponentDataArray<VoxelData>(Allocator.TempJob);
//        job.lookupWorldPositions = worldPositions;
//        job.lookupVoxelData = voxelData;
//        // Assign values to the fields on your job here, so that it has
//        // everything it needs to do its work when it runs later.
//        // For example,
//        //     job.deltaTime = UnityEngine.Time.deltaTime;
//
//
//        // Now that the job is set up, schedule it to be run. 
//        return job.Schedule(this, inputDependencies);
//    }
//}