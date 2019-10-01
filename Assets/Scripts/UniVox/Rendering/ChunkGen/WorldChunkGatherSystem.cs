using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UniVox.Managers.Game;
using UniVox.Types;
using UniVox.VoxelData;
using UniVox.VoxelData.Chunk_Components;

namespace UniVox
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class WorldChunkGatherSystem : JobComponentSystem
    {
        private EntityQuery _eventQuery;
        private EntityQuery _cleanupQuery;
        private EntityQuery _setupQuery;
        private EndInitializationEntityCommandBufferSystem _updateEnd;

        struct SystemVersion : ISystemStateComponentData
        {
            public uint Value;
        }

        protected override void OnCreate()
        {
            _eventQuery = GetEntityQuery(ComponentType.ReadOnly<ChunkIdComponent>(),
                ComponentType.ReadWrite<SystemVersion>());
            _setupQuery = GetEntityQuery(ComponentType.ReadOnly<ChunkIdComponent>(),
                ComponentType.Exclude<SystemVersion>());
            _cleanupQuery = GetEntityQuery(ComponentType.Exclude<ChunkIdComponent>(),
                ComponentType.ReadWrite<SystemVersion>());
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityManager.AddChunkComponentData(_setupQuery, new SystemVersion());
            EntityManager.RemoveComponent<SystemVersion>(_cleanupQuery);

            using (var chunks = _eventQuery.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                var PositionType = GetArchetypeChunkComponentType<ChunkIdComponent>();
                var EntityType = GetArchetypeChunkEntityType();
                var VersionType = GetArchetypeChunkComponentType<SystemVersion>();
                foreach (var chunk in chunks)
                {
                    var version = chunk.GetChunkComponentData(VersionType);
                    if (!chunk.DidChange(PositionType, version.Value))
                        continue;
                    
                    
                    inputDeps.Complete();
                    
                    chunk.SetChunkComponentData(VersionType,
                        new SystemVersion()
                        {
                            Value = chunk.GetComponentVersion(PositionType)
                        }
                    );


                    var positions = chunk.GetNativeArray(PositionType);
                    var entities = chunk.GetNativeArray(EntityType);
                    var worldId = positions[0].Value.WorldId;
                    if (!GameManager.Universe.TryGetValue(worldId, out var world))
                        continue;

                    world.ClearChunkEntities();
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        world.UpdateChunkEntity(positions[i].Value.ChunkId, entities[i]);
                    }
                }
            }


            return inputDeps;
//            inputDeps.Complete();
//
//            ProcessQuery();
//
//            return new JobHandle();
        }
    }
}