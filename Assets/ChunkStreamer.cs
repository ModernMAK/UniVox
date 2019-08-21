using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class ChunkStreamer : MonoBehaviour
{
    private int3 _lastChunk;
    [SerializeField] private int _renderDistance;
    [SerializeField] private Transform Target;
    private WorldBehaviour _wb;

    // Start is called before the first frame update
    void Start()
    {
        _wb = GetComponent<WorldBehaviour>();
        if (!Target.Equals(null) && !_wb.Equals(null))
            RequestChunks(GetCurrentChunkPos(), _renderDistance);
    }

    // Update is called once per frame
    void Update()
    {
        if (Target.Equals(null) || _wb.Equals(null))
            return;

        var cur = GetCurrentChunkPos();
        if (!_lastChunk.Equals(cur))
            RequestChunks(cur, _renderDistance);
    }

    void RequestChunks(int3 center, int distance, JobHandle handle = default)
    {
        var requests = CollectRequests(center, distance);
        var loaded = CollectLoaded();
        var shouldLoad = GatherChunksToLoad.Create(requests, loaded, out var load, handle);
        var shouldUnload = GatherChunksToUnload.Create(requests, loaded, out var unload, shouldLoad);
        shouldUnload.Complete();

        for (var i = 0; i < load.Length; i++)
            if (load[i])
                _wb.RequestLoad(requests[i]);

        for (var i = 0; i < unload.Length; i++)
            if (unload[i])
                _wb.RequestUnload(loaded[i]);


        requests.Dispose();
        loaded.Dispose();
        load.Dispose();
        unload.Dispose();

        _lastChunk = center;
    }

    private NativeArray<int3> CollectRequests(int3 center, int distance)
    {
        var realAxisSize = distance * 2 + 1;
        var flatSize = realAxisSize * realAxisSize * realAxisSize;
        var loading = new NativeArray<int3>(flatSize, Allocator.TempJob);

        var counter = 0;
        for (var x = -distance; x <= distance; x++)
        for (var y = -distance; y <= distance; y++)
        for (var z = -distance; z <= distance; z++)
        {
            loading[counter] = new int3(x, y, z) + center;
            counter++;
        }

        return loading;
    }

    private NativeArray<int3> CollectLoaded()
    {
        var count = _wb.LoadedCount;
        var loaded = _wb.Loaded.GetEnumerator();
        var arr = new NativeArray<int3>(count, Allocator.TempJob);
        for (var i = 0; i < count; i++)
        {
            if (loaded.MoveNext())
                arr[i] = loaded.Current;
        }

        loaded.Dispose();
        return arr;
    }


    public int3 GetCurrentChunkPos()
    {
        var realPos = (float3) Target.position;
        realPos /= Chunk.AxisSize;
        return new int3((int) math.floor(realPos.x), (int) math.floor(realPos.y), (int) math.floor(realPos.z));
    }
}

public struct GatherChunksToLoad : IJobParallelFor
{
    public static JobHandle Create(NativeArray<int3> requests, NativeArray<int3> loaded,
        out NativeArray<bool> shouldLoad, JobHandle handle = default)
    {
        shouldLoad = new NativeArray<bool>(requests.Length, Allocator.TempJob);
        return new GatherChunksToLoad()
        {
            ChunksRequested = requests,
            ChunksLoaded = loaded,
            ShouldLoad = shouldLoad
        }.Schedule(requests.Length, 64, handle);
    }

    [ReadOnly] public NativeArray<int3> ChunksRequested;

    [NativeDisableParallelForRestriction] [ReadOnly]
    public NativeArray<int3> ChunksLoaded;

    [WriteOnly] public NativeArray<bool> ShouldLoad;

    public void Execute(int index)
    {
        for (var i = 0; i < ChunksLoaded.Length; i++)
        {
            if (!ChunksLoaded[i].Equals(ChunksRequested[index])) continue;
            ShouldLoad[index] = false;
            return;
        }

        ShouldLoad[index] = true;
    }
}

public struct GatherChunksToUnload : IJobParallelFor
{
    public static JobHandle Create(NativeArray<int3> requests, NativeArray<int3> loaded,
        out NativeArray<bool> shouldUnload, JobHandle handle = default)
    {
        shouldUnload = new NativeArray<bool>(loaded.Length, Allocator.TempJob);
        return new GatherChunksToUnload()
        {
            ChunksRequested = requests,
            ChunksLoaded = loaded,
            ShouldUnload = shouldUnload
        }.Schedule(loaded.Length, 64, handle);
    }


    [NativeDisableParallelForRestriction] [ReadOnly]
    public NativeArray<int3> ChunksRequested;

    [ReadOnly] public NativeArray<int3> ChunksLoaded;

    [WriteOnly] public NativeArray<bool> ShouldUnload;

    public void Execute(int index)
    {
        for (var i = 0; i < ChunksRequested.Length; i++)
        {
            if (!ChunksRequested[i].Equals(ChunksLoaded[index])) continue;
            ShouldUnload[index] = true;
            return;
        }

        ShouldUnload[index] = false;
    }
}