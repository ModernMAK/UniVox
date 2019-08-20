using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class ChunkEntityPair : IDisposable
{
    private const int InnerBatchCount = 64;

    public const int ChunkSizePerAxis = ChunkData.ChunkSizePerAxis;
    public const int FlatSize = ChunkSizePerAxis * ChunkSizePerAxis * ChunkSizePerAxis;
    private readonly ChunkData Chunk;
    public NativeArray<Entity> EntityTable;

    public ChunkEntityPair(ChunkData chunkData)
    {
        EntityTable = new NativeArray<Entity>(FlatSize, Allocator.Persistent);
        Chunk = chunkData;
    }

    public void Dispose()
    {
        EntityTable.Dispose();
        Chunk?.Dispose();
    }


    public void Destroy()
    {
        var em = World.Active.EntityManager;
        em.DestroyEntity(EntityTable);
    }

    public void Spawn(Entity prefab)
    {
        World.Active.EntityManager.Instantiate(prefab, EntityTable);
    }


    private JobHandle SetupBlocks(int3 chunkPosition, EntityCommandBufferSystem bufferSystem,
        JobHandle dependencies = default)
    {
        var handle = new InitBlockJob
        {
            Buffer = bufferSystem.CreateCommandBuffer().ToConcurrent(),
            Entities = EntityTable,
            ChunkOffset = chunkPosition * ChunkSizePerAxis,
            RenderOffset = new float3(1f / 2f)
        }.Schedule(FlatSize, InnerBatchCount, dependencies);
        bufferSystem.AddJobHandleForProducer(handle);
        return handle;
    }

    private JobHandle SetupChunkActive(JobHandle dependencies = default)
    {
        return new SetupActiveJob
        {
            Solidity = Chunk.SolidTable
        }.Schedule(Chunk.SolidTable.ByteCount, InnerBatchCount, dependencies);
    }

    private JobHandle SetupChunkVisibility(JobHandle dependencies = default)
    {
        return new SetupVisibilityJob
        {
            HiddenFaces = Chunk.HiddenFaces
        }.Schedule(FlatSize, InnerBatchCount, dependencies);
    }

    private JobHandle SetupCulling(EntityCommandBufferSystem bufferSystem, JobHandle dependencies = default)
    {
        var handle = new SetupCulledJob
        {
            Buffer = bufferSystem.CreateCommandBuffer().ToConcurrent(),
            Entities = EntityTable,
            HiddenFaces = Chunk.HiddenFaces
        }.Schedule(FlatSize, InnerBatchCount, dependencies);
        bufferSystem.AddJobHandleForProducer(handle);
        return handle;
    }

    public JobHandle Init(int3 position)
    {
        var beginBarrier = World.Active.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        var endBarrier = World.Active.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();

//        var entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, World.Active);
        var setupActiveHandle = SetupChunkActive();
        var setupChunkVisible = SetupChunkVisibility(setupActiveHandle);
        var initHandle = SetupBlocks(position, beginBarrier, setupChunkVisible);
        var cullingHandle = SetupCulling(endBarrier, initHandle);
        return cullingHandle;
    }

    public void SetCulled(VoxelPos32 voxelPos32, bool culling)
    {
        var em = World.Active.EntityManager;
        var e = EntityTable[voxelPos32];
        if (culling)
        {
            if (!em.HasComponent(e, typeof(Disabled)))
                em.AddComponent(e, typeof(Disabled));
        }
        else
        {
            if (em.HasComponent(e, typeof(Disabled)))
                em.RemoveComponent(e, typeof(Disabled));
        }
    }
}