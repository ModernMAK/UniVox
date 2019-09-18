using System.Collections.Generic;
using System.ComponentModel;
using Jobs;
using Types;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEdits;
using UnityEngine;
using UniVox.Core.Types;
using UniVox.Core.Types.Universe;
using UniVox.Entities.Systems;

namespace UniVox.Core.Systems
{
    public class ChunkRenderSystem : JobComponentSystem
    {
        private struct ChunkRenderVersion : ISystemStateComponentData
        {
            public Version Info;
            public Version Render;
            public bool DidChange(Version info, Version render) => Info.DidChange(info) && Render.DidChange(render);
            public bool DidChange(Chunk chunk) => DidChange(chunk.Info.Version, chunk.Render.Version);

            public static ChunkRenderVersion Create(Chunk chunk)
            {
                return new ChunkRenderVersion()
                {
                    Info = chunk.Info.Version,
                    Render = chunk.Render.Version
                };
            }
        }

        private EntityQuery _renderQuery;
        private EntityQuery _setupQuery;
        private EntityQuery _cleanupQuery;

        private Universe _universe;
        private Material _material;

        private Dictionary<UniversalChunkId, NativeArray<Entity>> _entityCache;
        private EntityArchetype archetype;

        private void SetupArchetype()
        {
            archetype = EntityManager.CreateArchetype(
                typeof(RenderMesh),
                typeof(LocalToWorld));
        }

        protected override void OnCreate()
        {
            _universe = GameManager.Universe;
            _material = GameManager.MasterRegistry.Material[0];
            SetupArchetype();
            _renderQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                    ComponentType.ReadWrite<ChunkRenderVersion>()
                }
            });
            _setupQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                },
                None = new[]
                {
                    ComponentType.ReadWrite<ChunkRenderVersion>()
                }
            });
            _cleanupQuery = GetEntityQuery(new EntityQueryDesc()
            {
                None = new[]
                {
                    ComponentType.ReadOnly<ChunkIdComponent>(),
                },
                All = new[]
                {
                    ComponentType.ReadWrite<ChunkRenderVersion>()
                }
            });

            _entityCache = new Dictionary<UniversalChunkId, NativeArray<Entity>>();
        }

        protected override void OnDestroy()
        {
            //TODO also destory Entities
            _entityCache.Dispose();
            _entityCache.Clear();
        }


        NativeArray<Entity> GetResizedCache(UniversalChunkId id, int desiredLength)
        {
            ResizeCache(id, desiredLength);
            return _entityCache[id];
        }

        void ResizeCache(UniversalChunkId id, int desiredLength)
        {
            if (_entityCache.TryGetValue(id, out var cached))
            {
                if (cached.Length > desiredLength)
                {
                    var temp = new NativeArray<Entity>(desiredLength, Allocator.Persistent,
                        NativeArrayOptions.UninitializedMemory);
                    var excess = new NativeArray<Entity>(cached.Length - desiredLength, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);

                    NativeArray<Entity>.Copy(cached, temp, temp.Length);
                    NativeArray<Entity>.Copy(cached, temp.Length, excess, 0, excess.Length);

                    EntityManager.DestroyEntity(excess);
                    excess.Dispose();

                    _entityCache[id] = temp;
                    cached.Dispose();
                }
                else if (cached.Length < desiredLength)
                {
                    var temp = new NativeArray<Entity>(desiredLength, Allocator.Persistent,
                        NativeArrayOptions.UninitializedMemory);
                    var requested = new NativeArray<Entity>(desiredLength - cached.Length, Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);


                    NativeArray<Entity>.Copy(cached, temp, cached.Length);
                    EntityManager.CreateEntity(archetype, requested);
                    NativeArray<Entity>.Copy(requested, 0, temp, cached.Length, requested.Length);

                    requested.Dispose();

                    _entityCache[id] = temp;
                    cached.Dispose();
                }
            }
            else
            {
                var requested = new NativeArray<Entity>(desiredLength, Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory);
                EntityManager.CreateEntity(archetype, requested);
                _entityCache[id] = requested;
            }
        }

        void InitializeEntities(NativeArray<Entity> entities, float3 position)
        {
            var rotation = new float3x3(new float3(1, 0, 0), new float3(0, 1, 0), new float3(0, 0, 1));
            foreach (var entity in entities)
                EntityManager.SetComponentData(entity, new LocalToWorld() {Value = new float4x4(rotation, position)});
        }

        void RenderPass()
        {
            var chunkArray = _renderQuery.CreateArchetypeChunkArray(Allocator.TempJob);
            var idType = GetArchetypeChunkComponentType<ChunkIdComponent>(true);
            var versionType = GetArchetypeChunkComponentType<ChunkRenderVersion>();
            foreach (var ecsChunk in chunkArray)
            {
                var ids = ecsChunk.GetNativeArray(idType);
                var versions = ecsChunk.GetNativeArray(versionType);
                for (var i = 0; i < ecsChunk.Count; i++)
                {
                    var id = ids[i];
                    var version = versions[i];
                    if (!_universe.TryGetValue(id.Value.WorldId, out var world)) continue; //TODO produce an error
                    if (!world.TryGetAccessor(id.Value.ChunkId, out var record)) continue; //TODO produce an error
                    var voxelChunk = record.Chunk;
                    if (!version.DidChange(voxelChunk)) continue; //Skip this chunk


                    var meshes = CommonRenderingJobs.GenerateBoxelMeshes(voxelChunk);
                    var renderEntities = GetResizedCache(id.Value, meshes.Length);
                    InitializeEntities(renderEntities, id.Value.ChunkId); //ChunkID doubles as position


                    for (var j = 0; j < renderEntities.Length; j++)
                    {
                        var renderEntity = renderEntities[j];

                        var meshData = EntityManager.GetSharedComponentData<RenderMesh>(renderEntity);

                        meshData.mesh = meshes[j];
                        meshData.material = _material;

                        EntityManager.SetSharedComponentData(renderEntity, meshData);
                    }

                    //Update version
                    versions[i] = ChunkRenderVersion.Create(record.Chunk);
                }
            }
        }

        void SetupPass()
        {
            EntityManager.AddComponent<ChunkRenderVersion>(_setupQuery);
        }

        void CleanupPass()
        {
            EntityManager.RemoveComponent<ChunkRenderVersion>(_cleanupQuery);
            //TODO, lazy right now, but we need to cleanup the cache
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete();

            RenderPass();

            CleanupPass();
            SetupPass();

            return new JobHandle();
        }
    }
}