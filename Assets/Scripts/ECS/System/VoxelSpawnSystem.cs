//using ECS.Data.Voxel;
//using ECS.Voxel;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Transforms;
//using static Unity.Mathematics.math;
//
//
//public class VoxelSpawnSystem : JobComponentSystem
//{
//    struct VoxelSpawnUniverseJob : IJobForEachWithEntity<SpawnUniverseEvent>
//    {
//        // Add fields here that your job needs to do its work.
//        // For example,
//        //    public float deltaTime;
//        public EntityCommandBuffer.Concurrent CommandBuffer;
//
//        [ReadOnly] public bool HasUniverse;
//
//
//        public void Execute(Entity entity, int index, [ReadOnly] ref SpawnUniverseEvent data)
//        {
//            if (HasUniverse)
//            {
//                CommandBuffer.DestroyEntity(index, entity);
//                return;
//            }
//
//            var instance = CommandBuffer.Instantiate(index, data.UniversePrefab);
//
//            //We assume that we are going to have at most 255 chunks loaded at a time,
//            //because thats a good, arbitrary number
//            var map = new NativeHashMap<int3, Entity>(255, Allocator.Persistent);
//
//            CommandBuffer.SetSharedComponent(index, instance, new ChunkSize {value = data.ChunkSize});
//            CommandBuffer.SetSharedComponent(index, instance, new OldUniverseTable() {value = map});
//
//            CommandBuffer.DestroyEntity(index, entity);
//        }
//    }
//
//    struct VoxelSpawnChunkJob : IJobForEachWithEntity<SpawnChunkEvent>
//    {
//        // Add fields here that your job needs to do its work.
//        // For example,
//        //    public float deltaTime;
//        public EntityCommandBuffer.Concurrent CommandBuffer;
//        [ReadOnly] public Entity Universe;
//        [ReadOnly] public OldUniverseTable OldUniverseTable;
//
//
//        public void Execute(Entity entity, int index, [ReadOnly] ref SpawnChunkEvent data)
//        {
//            if (OldUniverseTable.value.TryGetValue(data.ChunkPosition, out var _))
//            {
//                CommandBuffer.DestroyEntity(index, entity);
//                return;
//            }
//
//            var instance = CommandBuffer.Instantiate(index, data.ChunkPrefab);
//            var chunkSize = data.ChunkSize;
//            var flatSize = chunkSize.x * chunkSize.y * chunkSize.z;
//            var map = new NativeHashMap<int3, Entity>(flatSize, Allocator.Persistent);
//
//            CommandBuffer.SetSharedComponent(index, instance, new ChunkSize {value = data.ChunkSize});
//            CommandBuffer.SetSharedComponent(index, instance, new OldChunkTable() {value = map});
//            CommandBuffer.SetSharedComponent(index, instance, new InUniverse() {value = Universe});
////            CommandBuffer.SetSharedComponent(index, instance, new InUniverse() {value = data.Universe});
//            CommandBuffer.SetComponent(index, instance, new OldChunkPosition() {value = data.ChunkPosition});
//
//            OldUniverseTable.value.TryAdd(data.ChunkPosition, instance);
//
////            CommandBuffer.AddComponent(index, instance, new FixChunkRelations());
//
//            CommandBuffer.DestroyEntity(index, entity);
//        }
//    }
//
//
//    struct VoxelSpawnVoxelJob : IJobForEachWithEntity<SpawnVoxelEvent>
//    {
//        // Add fields here that your job needs to do its work.
//        // For example,
//        //    public float deltaTime;
//        public EntityCommandBuffer.Concurrent CommandBuffer;
//        [ReadOnly] public Entity Universe;
//        [ReadOnly] public OldUniverseTable OldUniverseTable;
//
//
//        public void Execute(Entity entity, int index, [ReadOnly] ref SpawnVoxelEvent data)
//        {
//            if (!OldUniverseTable.value.TryGetValue(data.ChunkPosition, out var chunk))
//            {
//                CommandBuffer.DestroyEntity(index, entity);
//                return;
//            }
//
//
//            var instance = CommandBuffer.Instantiate(index, data.VoxelPrefab);
//
//            CommandBuffer.SetSharedComponent(index, instance, new ChunkSize {value = data.ChunkSize});
//            CommandBuffer.SetSharedComponent(index, instance, new OldVoxelChunkPosition() {value = data.ChunkPosition});
//            CommandBuffer.SetSharedComponent(index, instance, new InUniverse() {value = Universe});
////            CommandBuffer.SetSharedComponent(index, instance, new InUniverse() {value = data.Universe});
////            CommandBuffer.SetSharedComponent(index, instance, new InChunk() {value = data.Chunk});
//            CommandBuffer.SetSharedComponent(index, instance, new InChunk() {value = chunk});
//            CommandBuffer.SetComponent(index, instance, new VoxelPosition() {value = data.VoxelPosition});
//
//            CommandBuffer.AddComponent(index, instance, new FixVoxelRelations());
//
//
//            CommandBuffer.DestroyEntity(index, entity);
//        }
//    }
//
//    private BeginInitializationEntityCommandBufferSystem _bufferBarrier;
//
//    protected override void OnCreate()
//    {
//        base.OnCreate();
//        _bufferBarrier = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
//    }
//
//    protected override JobHandle OnUpdate(JobHandle inputDependencies)
//    {
//        // Schedule the job that will add Instantiate commands to the EntityCommandBuffer.
//        var spawnUniverseJobs = new VoxelSpawnUniverseJob
//        {
//            CommandBuffer = _bufferBarrier.CreateCommandBuffer().ToConcurrent()
//        }.Schedule(this, inputDependencies);
//
//        var spawnChunkJobs = new VoxelSpawnChunkJob
//        {
//            CommandBuffer = _bufferBarrier.CreateCommandBuffer().ToConcurrent()
//        }.Schedule(this, inputDependencies);
//
//        var spawnVoxelJobs = new VoxelSpawnVoxelJob()
//        {
//            CommandBuffer = _bufferBarrier.CreateCommandBuffer().ToConcurrent()
//        }.Schedule(this, inputDependencies);
//
//
//        // SpawnJob runs in parallel with no sync point until the barrier system executes.
//        // When the barrier system executes we want to complete the SpawnJob and then play back the commands (Creating the entities and placing them).
//        // We need to tell the barrier system which job it needs to complete before it can play back the commands.
//
//        var dependencies = JobHandle.CombineDependencies(spawnUniverseJobs, spawnChunkJobs, spawnVoxelJobs);
//        _bufferBarrier.AddJobHandleForProducer(dependencies);
//
//
//        // Now that the job is set up, schedule it to be run. 
//        return dependencies;
//    }
//}

