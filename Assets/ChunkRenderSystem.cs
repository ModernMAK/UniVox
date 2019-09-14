using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEdits.Rendering;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Math = Unity.Physics.Math;


[ExecuteAlways]
[AlwaysUpdateSystem]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public class ChunkRenderSystem : JobComponentSystem
{
    public struct State : ISystemStateComponentData
    {
        public uint BufferVersion;
        public uint ChunkPosVersion;
        public Guid CacheId;
    }

    private class RenderCache
    {
        public RenderCache(EntityManager em, EntityArchetype archetype)
        {
            _archetype = archetype;
            _manager = em;
            Entities = new List<NativeArray<Entity>>();
        }

        public void ResizePrimary(int size)
        {
            if (size > Entities.Count)
            {
                for (var i = Entities.Count; i < size; i++)
                {
                    Entities.Add(new NativeArray<Entity>(0, Allocator.Persistent,
                        NativeArrayOptions.UninitializedMemory));
                }
            }
            else if (size < Entities.Count)
            {
                for (var i = Entities.Count; i > size; i--)
                {
                    _manager.DestroyEntity(Entities[i]);
                    Entities[i].Dispose();
                    Entities.RemoveAt(i);
                }
            }
        }

        public void ResizeSecondary<T>(int primary, int size, NativeArray<T> elements) where T : struct
        {
            if (size == Entities[primary].Length)
                return;

            _manager.DestroyEntity(Entities[primary]);

            Entities[primary].Dispose();
            Entities[primary] = new NativeArray<Entity>(size, Allocator.Persistent);

            _manager.CreateEntity(_archetype, Entities[primary]);
        }

        private readonly EntityArchetype _archetype;
        private readonly EntityManager _manager;
        public List<NativeArray<Entity>> Entities { get; private set; }

        public void Dispose()
        {
            for (var i = 0; i < Entities.Count; i++)
            {
                _manager.DestroyEntity(Entities[i]);
                Entities[i].Dispose();
            }
        }
    }

    [BurstCompile]
    struct GatherRenderGroup : IJobParallelFor
    {
        [ReadOnly] public NativeArray<VoxelRenderChunkElement> Elements;

        [WriteOnly] public NativeArray<VoxelRenderChunkData.GroupId> Groups;

        public void Execute(int index)
        {
            Groups[index] = new VoxelRenderChunkData.GroupId(Elements[index]);
        }
    }

    struct RenderGroup
    {
        public Mesh Mesh;
        public Material Material;
    }

    private Dictionary<Guid, RenderCache> Caches;

    private void UpdateFullCache(RenderCache cache, ArchetypeChunk chunk,
        ArchetypeChunkComponentType<VoxelChunkPosition> voxelChunkPosType)
    {
        var entityType = GetArchetypeChunkEntityType();
        var bufferTable = GetBufferFromEntity<VoxelRenderChunkElement>(true);

        var entities = chunk.GetNativeArray(entityType);
        var voxelChunkPositions = chunk.GetNativeArray(voxelChunkPosType);


        for (var j = 0; j < chunk.Count; j++)
        {
            var buffer = bufferTable[entities[j]].AsNativeArray();
            FillPartialCache(j, voxelChunkPositions[j].Value, buffer, cache);
        }
    }

    private void FillPartialCache(int chunkId, int3 chunkPos, NativeArray<VoxelRenderChunkElement> chunkElements,
        RenderCache cache)
    {
        GatherRenderGroupIds(chunkElements, out var groupIds, out var sharedGroupIds).Complete();

        var renderGroups = GatherUniqueRenderGroups(sharedGroupIds);

        var chunkTransforms = GatherChunkTransforms(chunkPos);

//        var cacheIndex = cache.Meshes.Count;
//        cache.Materials.Add(new Material[renderGroups.Count]);
//        cache.Meshes.Add(new Mesh[renderGroups.Count]);
//        cache.Matrixes.Add(new float4x4[renderGroups.Count][]);

        cache.ResizeSecondary(chunkId, renderGroups.Count, chunkElements);

        for (var i = 0; i < renderGroups.Count; i++)
        {
            var groupedTransforms =
                GatherTransformGroup(i, chunkElements, chunkTransforms, sharedGroupIds, out var groupSize);


            var renderGroup = renderGroups[i];

            var entity = cache.Entities[chunkId][i];
            var renderMesh = EntityManager.GetSharedComponentData<RenderMesh>(entity);

            if (renderMesh.mesh != null)
            {
                var nativeMesh = renderGroup.Mesh.GetNativeMesh(Allocator.TempJob);

                if (renderGroup.Mesh.vertexCount * groupSize > ushort.MaxValue)
                    renderMesh.mesh.indexFormat = IndexFormat.UInt32;

                NativeMeshUtil.CreateMergeMeshJob(nativeMesh, groupedTransforms, groupSize, renderMesh.mesh);
                nativeMesh.Dispose();
            }

            renderMesh.material = renderGroup.Material;

            EntityManager.SetSharedComponentData(cache.Entities[chunkId][i], renderMesh);

            EntityManager.SetComponentData(cache.Entities[chunkId][i],
                new Translation() {Value = chunkPos * ChunkSize.AxisSize});
            groupedTransforms.Dispose();
        }

//        Profiler.EndSample();

        groupIds.Dispose();
        sharedGroupIds.Dispose();
        chunkTransforms.Dispose();
    }

//    private void RenderFromCache(RenderCache cache)
//    {
//        for (var i = 0; i < cache.Meshes.Count; i++)
//        for (var j = 0; j < cache.Meshes[i].Length; j++)
//            Graphics.DrawMeshInstanced(cache.Meshes[i][j], 0, cache.Materials[i][j], cache.Matrixes[i][j]);
//    }

    private JobHandle GatherRenderGroupIds(NativeArray<VoxelRenderChunkElement> chunkElements,
        out NativeArray<VoxelRenderChunkData.GroupId> groups,
        out NativeArraySharedValues<VoxelRenderChunkData.GroupId> sharedGroups)
    {
        Profiler.BeginSample("Gather Groups");
        groups = new NativeArray<VoxelRenderChunkData.GroupId>(chunkElements.Length, Allocator.TempJob,
            NativeArrayOptions.UninitializedMemory);

        var gatherJob = new GatherRenderGroup()
        {
            Elements = chunkElements,
            Groups = groups
        }.Schedule(chunkElements.Length, chunkElements.Length);
        Profiler.EndSample();

        Profiler.BeginSample("Sort Groups");
        sharedGroups = new NativeArraySharedValues<VoxelRenderChunkData.GroupId>(groups, Allocator.TempJob);
        var sharedJob = sharedGroups.Schedule(gatherJob);
        Profiler.EndSample();
        return sharedJob;
//        sharedJob.Complete();
    }

    private static readonly List<RenderGroup> GroupCache = new List<RenderGroup>();

    private IReadOnlyList<RenderGroup> GatherUniqueRenderGroups(
        NativeArraySharedValues<VoxelRenderChunkData.GroupId> sharedGroups)
    {
        var uniqueValuesInChunk = sharedGroups.SharedValueCount;
        var uniqueValueLengths = sharedGroups.GetSharedValueIndexCountArray();
        var uniqueValueIndexes = sharedGroups.GetSharedIndexArray();
        GroupCache.Clear();

        var offset = 0;
        for (var i = 0; i < uniqueValuesInChunk; i++)
        {
            var groupId = sharedGroups.SourceBuffer[uniqueValueIndexes[offset]];
            var meshFound = GameManager.MasterRegistry.Mesh.TryGetValue(groupId.MeshId, out var mesh);
            var materialFound = GameManager.MasterRegistry.Material.TryGetValue(groupId.MaterialId, out var material);

            GroupCache.Add(new RenderGroup()
            {
                Mesh = mesh,
                Material = material
            });
            offset += uniqueValueLengths[i];
        }

        return GroupCache;
    }


    private static readonly float3x3 Rotation =
        new float3x3(new float3(1, 0, 0), new float3(0, 1, 0), new float3(0, 0, 1));

    private NativeArray<float4x4> GatherChunkTransforms(int3 chunkPos)
    {
        var array = new NativeArray<float4x4>(ChunkSize.CubeSize, Allocator.TempJob,
            NativeArrayOptions.UninitializedMemory);


        var chunkOffset = chunkPos * ChunkSize.AxisSize;

        for (var i = 0; i < ChunkSize.CubeSize; i++)
        {
            var x = i % ChunkSize.AxisSize;
            var y = (i / ChunkSize.AxisSize) % ChunkSize.AxisSize;
            var z = i / ChunkSize.SquareSize;

            var position = new float3(x, y, z) + chunkOffset;

            array[i] = new float4x4(Rotation, position);
        }

        return array;
    }

    private NativeArray<float4x4> GatherTransformGroup(int index, NativeArray<VoxelRenderChunkElement> chunkElements,
        NativeArray<float4x4> transforms, NativeArraySharedValues<VoxelRenderChunkData.GroupId> sharedGroups,
        out int transformSize)
    {
        var sharedGroupSizes = sharedGroups.GetSharedValueIndexCountArray();
        var groupIndexes = sharedGroups.GetSortedIndices();
        var groupSize = sharedGroupSizes[index];

        var groupStart = 0;
        for (var i = 0; i < index; i++)
            groupStart += sharedGroupSizes[i];

        var groupTransforms =
            new NativeArray<float4x4>(groupSize, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        var culledCount = 0;
        for (var i = 0; i < groupSize; i++)
        {
            var trueIndex = groupIndexes[groupStart + i];
            if (chunkElements[trueIndex].Value.ShouldCullFlag)
            {
                culledCount++;
            }
            else
            {
                groupTransforms[i - culledCount] = transforms[trueIndex];
            }
        }

        transformSize = groupSize - culledCount;

        return groupTransforms;
    }

    private EntityQuery _cachedRenderQuery;
    private EntityQuery _uncachedRenderQuery;
    private EntityQuery _unmanagedQuery;

    protected override void OnCreate()
    {
        Caches = new Dictionary<Guid, RenderCache>();
        _cachedRenderQuery = GetEntityQuery(new EntityQueryDesc()
        {
            All = new[]
            {
                ComponentType.ReadWrite<VoxelRenderChunkElement>(), ComponentType.ReadWrite<VoxelChunkPosition>(),

                ComponentType.ChunkComponent<State>(),
            }
        });
        _uncachedRenderQuery = GetEntityQuery(new EntityQueryDesc()
        {
            All = new[]
            {
                ComponentType.ReadWrite<VoxelRenderChunkElement>(), ComponentType.ReadWrite<VoxelChunkPosition>(),
            },
            None = new[]
            {
                ComponentType.ChunkComponent<State>()
            }
        });
        _unmanagedQuery = GetEntityQuery(new EntityQueryDesc()
        {
            All = new[]
            {
                ComponentType.ChunkComponent<State>()
            },

            None = new[]
            {
                ComponentType.ReadWrite<VoxelRenderChunkElement>(), ComponentType.ReadWrite<VoxelChunkPosition>(),
            },
        });
        ChunkRenderArchetype = EntityManager.CreateArchetype(
            ComponentType.ReadWrite<RenderMesh>(),
            ComponentType.ReadWrite<Translation>(),
            ComponentType.ReadWrite<Rotation>(),
            ComponentType.ReadWrite<LocalToWorld>());
    }

    private void RenderCachedChunks()
    {
        var chunks = _cachedRenderQuery.CreateArchetypeChunkArray(Allocator.TempJob);
        var voxelChunkPosType = GetArchetypeChunkComponentType<VoxelChunkPosition>(true);
        var bufferType = GetArchetypeChunkBufferType<VoxelRenderChunkElement>(true);
        var stateType = GetArchetypeChunkComponentType<State>();
//        var cacheIdType = GetArchetypeChunkComponentType<RenderCacheId>(true);


        for (var i = 0; i < chunks.Length; i++)
        {
            var chunk = chunks[i];
            var state = chunk.GetChunkComponentData(stateType);
            var cache = GetCache(state.CacheId);


            if (chunk.DidChange(bufferType, state.BufferVersion) ||
                chunk.DidChange(voxelChunkPosType, state.ChunkPosVersion))
            {
                cache.ResizePrimary(chunk.Count);

                UpdateFullCache(cache, chunk, voxelChunkPosType);
                state.BufferVersion = chunk.GetComponentVersion(bufferType);
                state.ChunkPosVersion = chunk.GetComponentVersion(voxelChunkPosType);
                chunk.SetChunkComponentData(stateType, state);
            }

//            RenderFromCache(cache);
        }

        chunks.Dispose();
    }

    private RenderCache GetCache(Guid cacheId)
    {
        if (!Caches.TryGetValue(cacheId, out var cache))
        {
            cache = new RenderCache(EntityManager, ChunkRenderArchetype);
            Debug.LogWarning($"A Cache Was Generated Lazily for {cacheId}!");
        }

        return cache;
    }

    private void SetupChunkCache()
    {
        var query = _uncachedRenderQuery;

//        EntityManager.AddChunkComponentData(query, new RenderCacheId());


        var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob);


        EntityManager.AddChunkComponentData(query, new State());

//        var chunkPosType = GetArchetypeChunkComponentType<VoxelChunkPosition>();

        var stateId = GetArchetypeChunkComponentType<State>();
        for (var i = 0; i < chunks.Length; i++)
        {
            var id = Guid.NewGuid();


//            var value = chunks[i].GetChunkComponentData(chunkPosType).Value;
            var cache = Caches[id] = new RenderCache(EntityManager, ChunkRenderArchetype);

            chunks[i].SetChunkComponentData(stateId, new State() {CacheId = id});
        }

        chunks.Dispose();
    }

    public EntityArchetype ChunkRenderArchetype;

    private void CleanupUnmangedChunks()
    {
        var query = _unmanagedQuery;
        var chunks = query.CreateArchetypeChunkArray(Allocator.TempJob);

        var stateType = GetArchetypeChunkComponentType<State>();
        for (var i = 0; i < chunks.Length; i++)
        {
            var state = chunks[i].GetChunkComponentData(stateType);
            if (Caches.TryGetValue(state.CacheId, out var cache))
            {
                cache.Dispose();
            }
        }

        chunks.Dispose();

        EntityManager.RemoveChunkComponentData<State>(query);
//        EntityManager.RemoveChunkComponentData<RenderCacheId>(query);
//        EntityManager.RemoveComponent<RenderGroupCache>(_unmanagedQuery);
    }


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        inputDeps.Complete();


        RenderCachedChunks();
        SetupChunkCache();
        CleanupUnmangedChunks();

        return new JobHandle();
    }
}