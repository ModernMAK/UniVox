using System;
using System.Collections.Generic;
using ECS.Data.Voxel;
using ECS.Voxel.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class WorldBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject _target;
    [SerializeField] private int RenderDistance;
    [SerializeField] private GameObject _block;
    private int3 lastChunk = int3.zero;


    private const int ChunkSize = 32;

    private Dictionary<int3, ChunkEntityPair> ChunkTable;
    private Dictionary<int3, JobHandle> Handles;

    public void OnDestroy()
    {
        LoadOutAllChunks();
    }

    public void Start()
    {
        ChunkTable = new Dictionary<int3, ChunkEntityPair>();
        Handles = new Dictionary<int3, JobHandle>();
        lastChunk = ToChunkPos(_target.transform.position) + new int3(1);
    }

    public int3 ToChunkPos(Vector3 v)
    {
        var wPos = new int3(v);
        return wPos / ChunkSize;
    }

    public void Update()
    {
        var curPos = ToChunkPos(_target.transform.position);
        UpdateFromRenderDistance(curPos, RenderDistance);
    }

    private bool ChunkLoaded(int3 chunkPosition) => ChunkTable.ContainsKey(chunkPosition);


    public void LoadChunk(int3 chunkPosition)
    {
        var cep = ChunkTable[chunkPosition] = new ChunkEntityPair(new ChunkData());
//        cep.Spawn(_block, chunkPosition * ChunkSize);
        Handles[chunkPosition] = cep.Init(_block, chunkPosition);
    }

    public void UnloadChunk(int3 chunkPosition)
    {
        var cep = ChunkTable[chunkPosition];
        Handles[chunkPosition].Complete();
        cep.Destroy();
        cep.Dispose();
        ChunkTable.Remove(chunkPosition);
        Handles.Remove(chunkPosition);
    }


    public void UpdateFromRenderDistance(int3 chunkPos, int distance)
    {
        if (chunkPos.Equals(lastChunk))
            return;
        LoadOutChunks(chunkPos, distance);
        LoadInChunks(chunkPos, distance);
        lastChunk = chunkPos;
    }

    public void LoadOutAllChunks()
    {
        var temp = new Queue<int3>(ChunkTable.Keys);
        while (temp.Count > 0)
        {
            var pos = temp.Dequeue();
            ChunkTable[pos].Dispose();
        }
    }

    public void LoadOutChunks(int3 chunkPos, int distance)
    {
        var temp = new Queue<int3>(ChunkTable.Keys);
        while (temp.Count > 0)
        {
            var thisChunk = temp.Dequeue();
            var delta = chunkPos - thisChunk;
            var deltaAbs = math.abs(delta);
            if (deltaAbs.x > distance || deltaAbs.y > distance || deltaAbs.z > distance)
                UnloadChunk(thisChunk);
        }
    }

    public void LoadInChunks(int3 chunkPos, int distance)
    {
        for (var x = -distance; x <= distance; x++)
        for (var y = -distance; y <= distance; y++)
        for (var z = -distance; z <= distance; z++)
        {
            var pos = chunkPos + new int3(x, y, z);
            if (!ChunkLoaded(pos))
                LoadChunk(pos);
        }
    }
}

public class ChunkEntityPair : IDisposable
{
    public NativeArray<Entity> EntityTable;
    private ChunkData Chunk;

    [Obsolete("Use Init")]
    public void Spawn(GameObject prefab, int3 offset = default)
    {
        Debug.LogWarning("Deprecated");
        return;
    }

    public void Destroy()
    {
        var em = World.Active.EntityManager;
        foreach (var pos in VoxPos.GetAllPositions())
        {
            em.DestroyEntity(EntityTable[pos]);
        }
    }

    public JobHandle Init(GameObject prefab, int3 position)
    {
        var barrier = World.Active.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();

        var entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, World.Active);
        var spawnJob = new SpawnEntitiesJob()
        {
            Buffer = barrier.CreateCommandBuffer().ToConcurrent(),
            ChunkOffset = position * 32,
            RenderOffset = new float3(1f / 2f),
            Prefab = entityPrefab
        };
        var spawnHandle = spawnJob.Schedule(FlatSize, 64);

        var setupActiveJob = new SetupActiveJob() {Solidity = Chunk.SolidTable};
        var activeHandle = setupActiveJob.Schedule(Chunk.SolidTable.ByteCount, 64);

        var setupVisibilityJob = new SetupVisibilityJob() {HiddenFaces = Chunk.HiddenFaces};
        var visibleHandle = setupVisibilityJob.Schedule(FlatSize, 64);

        var combined = JobHandle.CombineDependencies(activeHandle, visibleHandle, spawnHandle);
        var cullJob = new SetupCulledJob()
        {
            Buffer = barrier.CreateCommandBuffer().ToConcurrent(),
            Entities = EntityTable,
            HiddenFaces = Chunk.HiddenFaces
        };
        var cullHandle = cullJob.Schedule(FlatSize, 64, combined);
        return cullHandle;
    }

    public void SetCulled(VoxPos voxPos, bool culling)
    {
        var em = World.Active.EntityManager;
        var e = EntityTable[voxPos];
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

    public ChunkEntityPair(ChunkData chunkData)
    {
        EntityTable = new NativeArray<Entity>(FlatSize, Allocator.Persistent);
        Chunk = chunkData;
    }

    public static T Get<T>(T[,,] data, int3 pos) => data[pos.x, pos.y, pos.z];
    public static void Set<T>(T[,,] data, int3 pos, T value) => data[pos.x, pos.y, pos.z] = value;
    public const int ChunkSizePerAxis = 32;
    public const int FlatSize = ChunkSizePerAxis * ChunkSizePerAxis * ChunkSizePerAxis;

    public void Dispose()
    {
        EntityTable.Dispose();
        Chunk?.Dispose();
    }
}

struct SetupActiveJob : IJobParallelFor
{
    [WriteOnly] public NativeBitArray Solidity;

    public void Execute(int index)
    {
//        for (var i = 0; i < 8; i++)
//            SolidityWrite[index * 8 + i] = true;
        Solidity.SetByte(index, byte.MaxValue);
    }
}

struct SetupVisibilityJob : IJobParallelFor
{
    [WriteOnly] public NativeArray<Directions> HiddenFaces;

    public void Execute(int index)
    {
        var position = new VoxPos(index).Position;
        var flags = DirectionsX.NoneFlag;

        if (position.x != 0)
            flags |= Directions.Left;
        if (position.x != VoxPos.MaxValue)
            flags |= Directions.Right;

        if (position.y != 0)
            flags |= Directions.Down;
        if (position.y != VoxPos.MaxValue)
            flags |= Directions.Up;

        if (position.z != 0)
            flags |= Directions.Backward;
        if (position.z != VoxPos.MaxValue)
            flags |= Directions.Forward;


        HiddenFaces[index] = flags;
    }
}

struct SetupCulledJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Directions> HiddenFaces;
    [ReadOnly] public NativeArray<Entity> Entities;

    public EntityCommandBuffer.Concurrent Buffer;

    public void Execute(int index)
    {
        var e = Entities[index];
        var culling = HiddenFaces[index].IsAll();
        if (culling)
        {
            Buffer.AddComponent<Disabled>(index, e);
        }
        else
        {
            Buffer.RemoveComponent<Disabled>(index, e);
        }
    }
}

struct SpawnEntitiesJob : IJobParallelFor
{
    [WriteOnly] public NativeArray<Entity> Entities;
    [ReadOnly] public Entity Prefab;
    [ReadOnly] public float3 RenderOffset;
    [ReadOnly] public int3 ChunkOffset;
    public EntityCommandBuffer.Concurrent Buffer;

    public void Execute(int index)
    {
        var voxPos = new VoxPos(index);
        var e = Entities[index] = Buffer.Instantiate(index, Prefab);
        Buffer.SetComponent(index, e, new Translation() {Value = voxPos.Position + RenderOffset + ChunkOffset});

        Buffer.AddComponent<Disabled>(index, e);
    }
}