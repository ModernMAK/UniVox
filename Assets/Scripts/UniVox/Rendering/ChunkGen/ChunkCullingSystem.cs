using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEdits;
using UnityEdits.Hybrid_Renderer;
using UnityEngine.Profiling;
using UniVox.Rendering.ChunkGen.Jobs;
using UniVox.Types;
using UniVox.VoxelData.Chunk_Components;

namespace UniVox.Rendering.ChunkGen
{
    public static class ChunkComponentVersionX
    {
        public static void DirtyComponent<T>(this EntityManager em, Entity entity)
            where T : struct, IVersionProxy<T>, IComponentData
        {
            var data = em.GetComponentData<T>(entity);
            em.SetComponentData<T>(entity, data.GetDirty());
        }

        public static void DirtySystemComponent<T>(this EntityManager em, Entity entity)
            where T : struct, IVersionProxy<T>, ISystemStateComponentData
        {
            var data = em.GetComponentData<T>(entity);
            em.SetComponentData<T>(entity, data.GetDirty());
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(ChunkInitializationSystem))]
    [UpdateBefore(typeof(ChunkMeshGenerationSystem))]
    public class ChunkCullingSystem : JobComponentSystem
    {
        public struct ChunkCullingSystemVersion : ISystemStateComponentData
        {
            public uint ActiveVersion;


            public bool DidChange(ChunkCullingSystemVersion version)
            {
                return ChangeVersionUtility.DidChange(ActiveVersion, version.ActiveVersion);
            }

            public override string ToString()
            {
                return ActiveVersion.ToString();
            }
        }


        private EntityQuery _renderQuery;
        private EntityQuery _setupQuery;
        private EntityQuery _cleanupQuery;


        protected override void OnCreate()
        {
            _renderQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                    ComponentType.ReadWrite<ChunkCullingSystemVersion>(),

                    ComponentType.ReadWrite<BlockCulledFacesComponent>(),
                    ComponentType.ReadOnly<BlockActiveComponent>(),

                    ComponentType.ReadOnly<BlockCulledFacesComponent.Version>(),
                    ComponentType.ReadOnly<BlockActiveComponent.Version>(),
                }
            });
            _setupQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),

                    ComponentType.ReadWrite<BlockCulledFacesComponent>(),
                    ComponentType.ReadOnly<BlockActiveComponent>(),

                    ComponentType.ReadOnly<BlockCulledFacesComponent.Version>(),
                    ComponentType.ReadOnly<BlockActiveComponent.Version>(),
                },
                None = new[]
                {
                    ComponentType.ReadWrite<ChunkCullingSystemVersion>()
                }
            });
            _cleanupQuery = GetEntityQuery(new EntityQueryDesc()
            {
                None = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),


                    ComponentType.ReadWrite<BlockCulledFacesComponent>(),
                    ComponentType.ReadOnly<BlockActiveComponent>(),

                    ComponentType.ReadOnly<BlockCulledFacesComponent.Version>(),
                    ComponentType.ReadOnly<BlockActiveComponent.Version>(),
                },
                All = new[]
                {
                    ComponentType.ReadWrite<ChunkCullingSystemVersion>()
                }
            });

//            BlockActive = GetBufferFromEntity<BlockActiveComponent>(true );
//            BlockActive = GetBufferFromEntity<BlockActiveComponent>();
        }

        JobHandle RenderPass(JobHandle dependencies = default)
        {
            var chunkArray = _renderQuery.CreateArchetypeChunkArray(Allocator.TempJob);
//            var activeType = GetArchetypeChunkBufferType<BlockActiveComponent>(true);
            var versionType = GetArchetypeChunkComponentType<ChunkCullingSystemVersion>();
            var blockCulledVersionType = GetArchetypeChunkComponentType<BlockCulledFacesComponent.Version>();
//            var changedType = GetArchetypeChunkBufferType<BlockChanged>();

            var blockActiveVersionType = GetArchetypeChunkComponentType<BlockActiveComponent.Version>(true);

            var VoxelChunkEntityArchetype = GetArchetypeChunkEntityType();

//            var merged = new JobHandle();
            Profiler.BeginSample("Process ECS Chunk");
            foreach (var ecsChunk in chunkArray)
            {
                var systemVersions = ecsChunk.GetNativeArray(versionType);
                var activeVersions = ecsChunk.GetNativeArray(blockActiveVersionType);
                var blockCulledVersions = ecsChunk.GetNativeArray(blockCulledVersionType);
                var voxelChunkEntityArray = ecsChunk.GetNativeArray(VoxelChunkEntityArchetype);

                var i = 0;
                foreach (var voxelChunkEntity in voxelChunkEntityArray)
                {
                    var version = systemVersions[i];
                    var currentVersion = new ChunkCullingSystemVersion()
                    {
                        ActiveVersion = activeVersions[i]
                    };

                    if (currentVersion.DidChange(version))
                    {
                        Profiler.BeginSample("Update Chunk");
//                        var job
                        var job = UpdateVoxelChunkV2(voxelChunkEntity, dependencies);
//
                        job.Complete();
                        Profiler.EndSample();
                        systemVersions[i] = currentVersion;
                        blockCulledVersions[i] = blockCulledVersions[i].GetDirty();
                    }

                    i++;
                }
            }


            Profiler.EndSample();

            chunkArray.Dispose();
//            return merged;
            return dependencies;
        }

        [BurstCompile]
        private struct CullFacesJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<BlockActiveComponent> BlockActive;
            [WriteOnly] public NativeArray<BlockCulledFacesComponent> CulledFaces;

            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Direction> Directions;

//            private int3 ToPosition(int index)
//            {
//                var axisSize = UnivoxDefine.AxisSize;
//                var x = index % axisSize;
//                var y = index / axisSize % axisSize;
//                var z = index / (axisSize * axisSize);
//                return new int3(x, y, z);
//            }
//            private int ToIndex(int3 position)
//            {
//                var axisSize = UnivoxDefine.AxisSize;
//                return position.x + position.y * axisSize + position.z * axisSize * axisSize;
//            }

            public void Execute(int blockIndex)
            {
                var blockPos = UnivoxUtil.GetPosition3(blockIndex);
//                Profiler.BeginSample("Process Block");

                var primaryActive = BlockActive[blockIndex];

                var hidden = DirectionsX.AllFlag;
                var directions = Directions; //DirectionsX.GetDirectionsNative(Allocator.Temp);

                for (var dirIndex = 0; dirIndex < Directions.Length; dirIndex++)
                {
                    var direction = directions[dirIndex];
                    var neighborPos = blockPos + direction.ToInt3();
                    var neighborIndex = UnivoxUtil.GetIndex(neighborPos);
                    var neighborActive = false;
                    if (UnivoxUtil.IsPositionValid(neighborPos))
                    {
                        neighborActive = BlockActive[neighborIndex];
                    }

                    if (primaryActive && !neighborActive)
                    {
                        hidden &= ~direction.ToFlag();
                    }
                }

                CulledFaces[blockIndex] = hidden;
//                directions.Dispose();


//                Profiler.EndSample();
            }
        }


//        private BufferFromEntity<BlockActiveComponent> BlockActive;
//        private BufferFromEntity<BlockCulledFacesComponent> CulledFaces;

        private JobHandle UpdateVoxelChunkV2(Entity voxelChunk, JobHandle dependencies = default)
        {
            var blockActiveLookup = GetBufferFromEntity<BlockActiveComponent>(true);
            var blockCulledLookup = GetBufferFromEntity<BlockCulledFacesComponent>();
            var blockActive = blockActiveLookup[voxelChunk];
            var blockCulled = blockCulledLookup[voxelChunk];

            var job = new CullFacesJob()
            {
                Directions = DirectionsX.GetDirectionsNative(Allocator.TempJob),
                BlockActive = blockActive.AsNativeArray(),
                CulledFaces = blockCulled.AsNativeArray(),
            };
            return job.Schedule(UnivoxDefine.CubeSize, UnivoxDefine.AxisSize, dependencies);
        }


        void SetupPass()
        {
            EntityManager.AddComponent<ChunkCullingSystemVersion>(_setupQuery);
        }

        void CleanupPass()
        {
            EntityManager.RemoveComponent<ChunkCullingSystemVersion>(_cleanupQuery);
            //TODO, lazy right now, but we need to cleanup the cache
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete();


            CleanupPass();
            SetupPass();

            var job = RenderPass(inputDeps);
//            job.Complete();
//            return new JobHandle();
            return job;
//            return job; // new JobHandle();
        }
    }
}