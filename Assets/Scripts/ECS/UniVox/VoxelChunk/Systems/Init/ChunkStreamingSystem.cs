//using System;
//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Transforms;
//using UniVox;
//using UniVox.Types;
//
//namespace ECS.UniVox.Systems
//{
//    public struct ChunkStreamingTarget : IComponentData
//    {
//        public int3 Distance;
//    }
//
//    public class ChunkStreamingSystem : JobComponentSystem
//    {
//        private ChunkCreationProxy _creationProxy;
//        private EntityQuery _query;
//        private byte _worldId;
//        private WorldMap _worldMap;
//
//        protected override void OnCreate()
//        {
//            _creationProxy = new ChunkCreationProxy(World.Active);
//            _worldMap = GameManager.Universe.GetOrCreate(World.Active, out _worldId);
//            _query = GetEntityQuery(new EntityQueryDesc
//            {
//                All = new[]
//                {
//                    ComponentType.ReadOnly<LocalToWorld>(),
//                    ComponentType.ReadOnly<ChunkStreamingTarget>()
//                }
//            });
//        }
//
//
//        protected JobHandle LoadPass(EntityQuery query, JobHandle inputDeps)
//        {
//            const Allocator jobAlloc = Allocator.TempJob;
//            var requests = new NativeList<ChunkPosition>(jobAlloc);
//            var unique = new NativeList<ChunkPosition>(jobAlloc);
//            var loadedFlags = new NativeList<bool>(jobAlloc);
//            var unloadedRequests = new NativeList<ChunkPosition>(jobAlloc);
//
//            var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob, out var createHandle);
//            inputDeps = JobHandle.CombineDependencies(inputDeps, createHandle);
//
//
//            //GATHER EVERY REQUEST IN THE QUERY
//            inputDeps = new GatherRequestsJob
//            {
//                Chunks = chunks,
//                Requests = requests,
//                LocalToWorldType = GetArchetypeChunkComponentType<LocalToWorld>(),
//                StreamTargetType = GetArchetypeChunkComponentType<ChunkStreamingTarget>()
//            }.Schedule(inputDeps);
////            inputDeps = new DisposeArrayJob<ArchetypeChunk>(chunks).Schedule(inputDeps);
////            inputDeps = requests.Dispose(inputDeps);
////            return inputDeps; //TEST to see how far we get before we crash
//
//            //FILTER OUT DUPLICATES
//            inputDeps = new FindUniquesJob<ChunkPosition>
//            {
//                Source = requests.AsDeferredJobArray(),
//                Unique = unique
//            }.Schedule(inputDeps);
//
//            //cleanup
//            inputDeps = requests.Dispose(inputDeps);
//
//            //'Load' map
//            inputDeps = _worldMap.GetNativeMapDependency(inputDeps);
//            var map = _worldMap.GetNativeMap();
//
//
//            //DETERMINE WHAT IS LOADED
//
//            inputDeps = new MapHasKeyJob<ChunkPosition, Entity>
//            {
//                Map = map,
//                Keys = unique.AsDeferredJobArray(),
//                Results = loadedFlags
//            }.Schedule(inputDeps);
//
//            //FILTER OUT LOADED (we want unloaded)
//            inputDeps = new FilterJob<ChunkPosition>
//            {
//                FilteredValues = unloadedRequests,
//                Filters = loadedFlags.AsDeferredJobArray(),
//                FilterToMatch = false,
//                Values = unique.AsDeferredJobArray()
//            }.Schedule(inputDeps);
//
//            //cleanup
//            inputDeps = loadedFlags.Dispose(inputDeps);
//            inputDeps = unique.Dispose(inputDeps);
//
//            //LOAD UNLOADED CHUNKS
//            inputDeps = _worldMap.GetNativeMapDependency(inputDeps);
//            inputDeps = _creationProxy.CreateChunks(_worldId, unloadedRequests.AsDeferredJobArray(), inputDeps);
//
//            //'Release' map
//            _worldMap.AddNativeMapDependency(inputDeps);
//
//            //cleanup
//            inputDeps = unloadedRequests.Dispose(inputDeps);
//            inputDeps = new DisposeArrayJob<ArchetypeChunk>(chunks).Schedule(inputDeps);
//
//            return inputDeps;
//        }
//
//
//        protected override JobHandle OnUpdate(JobHandle inputDeps)
//        {
////            return inputDeps;
//            try
//            {
//                inputDeps = LoadPass(_query, inputDeps);
//            }
//            catch (Exception e)
//            {
//                throw e;
//            }
//
//            return inputDeps;
//        }
//
//        [BurstCompile]
//        private struct GatherRequestsJob : IJob
//        {
//            //Because we really want int3 to be comparable
//            //Also it IS more representative than an int3
//            public NativeList<ChunkPosition> Requests;
//
//            [ReadOnly] public NativeArray<ArchetypeChunk> Chunks;
//
//            [ReadOnly] public ArchetypeChunkComponentType<ChunkStreamingTarget> StreamTargetType;
//            [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> LocalToWorldType;
//
//
//            public void Execute()
//            {
//                for (var chunkIndex = 0; chunkIndex < Chunks.Length; chunkIndex++)
//                {
//                    var chunk = Chunks[chunkIndex];
//                    var streamData = chunk.GetNativeArray(StreamTargetType);
//                    var positionData = chunk.GetNativeArray(LocalToWorldType);
//
//                    for (var entityIndex = 0; entityIndex < chunk.Count; entityIndex++)
//                    {
//                        var unitySpaceWorldPosition = positionData[entityIndex].Position;
//                        var voxelSpaceWorldPosition = UnivoxUtil.ToVoxelSpace(unitySpaceWorldPosition);
//                        var voxelSpaceChunkPosition = UnivoxUtil.ToChunkPosition(voxelSpaceWorldPosition);
//
//                        var distance = streamData[entityIndex].Distance;
//                        for (var dx = -distance.x; dx <= distance.x; dx++)
//                        for (var dy = -distance.y; dy <= distance.y; dy++)
//                        for (var dz = -distance.z; dz <= distance.z; dz++)
//                            Requests.Add(voxelSpaceChunkPosition + new int3(dx, dy, dz));
//                    }
//                }
//            }
//        }
//
//        [BurstCompile]
//        private struct MapHasKeyJob<TKey, TValue> : IJob where TKey : struct, IEquatable<TKey> where TValue : struct
//        {
//            [ReadOnly] public NativeArray<TKey> Keys;
//            [ReadOnly] public NativeHashMap<TKey, TValue> Map;
//            public NativeList<bool> Results;
//
//            public void Execute()
//            {
//                for (var keyIndex = 0; keyIndex < Keys.Length; keyIndex++)
//                {
//                    var key = Keys[keyIndex];
//                    var hasKey = Map.ContainsKey(key);
//                    Results.Add(hasKey);
//                }
//            }
//        }
//
//        [BurstCompile]
//        private struct FilterJob<TKey> : IJob where TKey : struct
//        {
//            [ReadOnly] public NativeArray<TKey> Values;
//            [ReadOnly] public NativeArray<bool> Filters;
//            [ReadOnly] public bool FilterToMatch;
//            public NativeList<TKey> FilteredValues;
//
//            public void Execute()
//            {
//                for (var valueIndex = 0; valueIndex < Values.Length; valueIndex++)
//                {
//                    //This looks really dumb (match could be  one line, why store filter? etc)
//                    //Its to make stepping through the job easier when debugging
//                    var filter = Filters[valueIndex];
//                    var match = FilterToMatch == filter;
//                    if (match)
//                    {
//                        var value = Values[valueIndex];
//                        FilteredValues.Add(value);
//                    }
//                }
//            }
//        }
//    }
//}