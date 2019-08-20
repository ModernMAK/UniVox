using System;
using System.Collections.Generic;
using ECS.Data.Voxel;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class WorldBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject _target;
    [SerializeField] private int RenderDistance;
    [SerializeField] private GameObject _block;
    private int3 lastChunk = int3.zero;
    private Entity _prefab;


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
        _prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(_block, World.Active);
    }

    private NativeArray<int3> GetListLoaded(Allocator allocator)
    {
        var map = new NativeArray<int3>(ChunkTable.Count, allocator);
        var i = 0;
        foreach (var key in ChunkTable.Keys)
        {
            map[i] = key;
            i++;
        }

        return map;
    }

    [BurstCompile]
    struct GatherChunksToUnload : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int3> LoadedChunks;
        [ReadOnly] public int3 ReferencePosition;
        [ReadOnly] public int Distance;
        [WriteOnly] public NativeQueue<int3>.ParallelWriter ChunksToUnload;

        public void Execute(int index)
        {
            var chunkPos = LoadedChunks[index];
            var delta = chunkPos - ReferencePosition;
            var absDelta = math.abs(delta);
            if (absDelta.x > Distance || absDelta.y > Distance || absDelta.z > Distance)
            {
                ChunksToUnload.Enqueue(chunkPos);
            }
        }
    }

    [BurstCompile]
    struct GatherChunksToLoad : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int3> LoadedChunks;
        [ReadOnly] public int Distance;
        [WriteOnly] public NativeQueue<int3>.ParallelWriter ChunksToLoad;


        private int AxisSize => Distance * 2 + 1;

        public void Execute(int index)
        {
            //-D -> D
            // i -> 2D+1
            var x = index % AxisSize;
            var y = (index / AxisSize) % AxisSize;
            var z = (index / AxisSize / AxisSize) % AxisSize;

            x -= Distance;
            y -= Distance;
            z -= Distance;

            var inspectPos = new int3(x, y, z);
            if (!LoadedChunks.Contains(inspectPos))
            {
                ChunksToLoad.Enqueue(inspectPos);
            }
        }
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
        cep.Spawn(_prefab);
        Handles[chunkPosition] = cep.Init(chunkPosition);
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
        LoadOutChunksJobified(chunkPos, distance);
        LoadInChunksJobified(chunkPos, distance);
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

    [Obsolete]
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

    public void LoadOutChunksJobified(int3 chunkPos, int distance)
    {
        var list = new NativeQueue<int3>(Allocator.TempJob);
        var loaded = GetListLoaded(Allocator.TempJob);
        var handle = new GatherChunksToUnload()
        {
            ChunksToUnload = list.AsParallelWriter(),
            Distance = RenderDistance,
            LoadedChunks = loaded,
            ReferencePosition = chunkPos
        }.Schedule(loaded.Length, 64);
        handle.Complete();

        while (list.Count > 0)
            UnloadChunk(list.Dequeue());

        list.Dispose();
        loaded.Dispose();
//        var temp = new Queue<int3>(ChunkTable.Keys);
//        while (temp.Count > 0)
//        {
//            var thisChunk = temp.Dequeue();
//            var delta = chunkPos - thisChunk;
//            var deltaAbs = math.abs(delta);
//            if (deltaAbs.x > distance || deltaAbs.y > distance || deltaAbs.z > distance)
//                UnloadChunk(thisChunk);
//        }
    }

    [Obsolete]
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

    public void LoadInChunksJobified(int3 chunkPos, int distance)
    {
        var perSize = (distance * 2 + 1);
        var fullSize = perSize * perSize * perSize;
        var list = new NativeQueue<int3>(Allocator.TempJob);
        var loaded = GetListLoaded(Allocator.TempJob);
        var handle = new GatherChunksToLoad()
        {
            Distance = RenderDistance,
            LoadedChunks = loaded,
            ChunksToLoad = list.AsParallelWriter()
        }.Schedule(fullSize, 64);
        handle.Complete();
//        for (var i = 0; i < list.Length; i++)
        while (list.Count > 0)
            LoadChunk(list.Dequeue());

        list.Dispose();
        loaded.Dispose();
    }
}