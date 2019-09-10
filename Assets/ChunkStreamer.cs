//using System;
//using System.Collections.Generic;
//using Unity.Collections;
//using Unity.Collections.LowLevel.Unsafe;
//using Unity.Jobs;
//using Unity.Mathematics;
//using UnityEngine;
//
//
//[NativeContainer]
//[NativeContainerSupportsMinMaxWriteRestriction]
//public struct NativeHashSet<T> : IDisposable where T : struct, IEquatable<T>
//{
//    private const bool Item = true;
//    private NativeHashMap<T, bool> _backing;
//
//
//    public NativeHashSet(int size, Allocator allocator)
//    {
//        _backing = new NativeHashMap<T, bool>(size, allocator);
//#if ENABLE_UNITY_COLLECTIONS_CHECKS
//        m_Length = 0;
//        m_MinIndex = 0;
//        m_MaxIndex = size - 1;
//        DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, allocator);
//#endif
//    }
//
//    public NativeHashSet(NativeArray<T> array, Allocator allocator) : this(array.Length, allocator)
//    {
//        for (var i = 0; i < array.Length; i++)
//            Add(array[i]);
//    }
//
//    public NativeHashSet(IList<T> array, Allocator allocator) : this(array.Count, allocator)
//    {
//        for (var i = 0; i < array.Count; i++)
//            Add(array[i]);
//    }
//
//    public NativeHashSet(IEnumerable<T> array, int size, Allocator allocator) : this(size, allocator)
//    {
//        var enumerator = array.GetEnumerator();
//        while (enumerator.MoveNext())
//            Add(enumerator.Current);
//        enumerator.Dispose();
//    }
//
//
//#if ENABLE_UNITY_COLLECTIONS_CHECKS
//    internal int m_Length;
//    internal int m_MinIndex;
//    internal int m_MaxIndex;
//    internal AtomicSafetyHandle m_Safety;
//    [NativeSetClassTypeToNullOnSchedule] internal DisposeSentinel m_DisposeSentinel;
//#endif
//
//    public int Capacity => _backing.Capacity;
//    public int Length => _backing.Length;
//
//
//    public void Add(T value)
//    {
//        var result = _backing.TryAdd(value, Item);
//        m_Length = _backing.Length;
//        m_MaxIndex = m_Length - 1;
//    }
//
//    public bool Contains(T value)
//    {
//        return _backing.ContainsKey(value);
//    }
//
//    public void Remove(T value)
//    {
//        _backing.Remove(value);
//        m_Length = _backing.Length;
//        m_MaxIndex = m_Length - 1;
//    }
//
//    public void Clear()
//    {
//        _backing.Clear();
//        m_Length = _backing.Length;
//        m_MaxIndex = m_Length - 1;
//    }
//
//    public NativeArray<T> ToArray(Allocator allocator)
//    {
//        return _backing.GetKeyArray(allocator);
//    }
//
//    public bool IsCreated => _backing.IsCreated;
//
//    public void Dispose()
//    {
//#if ENABLE_UNITY_COLLECTIONS_CHECKS
//        DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
//#endif
//        _backing.Dispose();
//    }
//}
//
//public class ChunkStreamer : MonoBehaviour
//{
//    private int3 _lastChunk;
//    [SerializeField] private int _renderDistance;
//    private WorldBehaviour _wb;
//    [SerializeField] private Transform Target;
//
//    // Start is called before the first frame update
//    private void Start()
//    {
//        _wb = GetComponent<WorldBehaviour>();
//        if (!Target.Equals(null) && !_wb.Equals(null))
//            RequestChunks(GetCurrentChunkPos(), _renderDistance);
//    }
//
//    // Update is called once per frame
//    private void Update()
//    {
//        if (Target.Equals(null) || _wb.Equals(null))
//            return;
//
//        var cur = GetCurrentChunkPos();
//        if (!_lastChunk.Equals(cur))
//            RequestChunks(cur, _renderDistance);
//    }
//
//    private void RequestChunks(int3 center, int distance, JobHandle handle = default)
//    {
//        var requests = CollectRequests(center, distance);
//        var loaded = new NativeHashSet<int3>(_wb.Loaded, _wb.LoadedCount, Allocator.TempJob);
//        var loadedFlat = loaded.ToArray(Allocator.TempJob);
//
//        var reqFoci = new NativeArray<int3>(1, Allocator.TempJob);
//        reqFoci[0] = center;
//        var reqDis = new NativeArray<int>(1, Allocator.TempJob);
//        reqDis[0] = distance;
//
//
//        var shouldLoad = GatherChunksToLoadV2.Create(requests, loaded, out var load, handle);
//
//        var shouldUnload = GatherChunksToUnloadV2.Create(reqFoci, reqDis, loadedFlat, out var unload, shouldLoad);
//
//
//        shouldUnload.Complete();
//
//
//        while (load.TryDequeue(out var item))
//            _wb.RequestLoad(item);
//
//        while (unload.TryDequeue(out var item))
//            _wb.RequestUnload(item);
//
//        reqDis.Dispose();
//        reqFoci.Dispose();
//
//        requests.Dispose();
//
//        loaded.Dispose();
//        loadedFlat.Dispose();
//
//        load.Dispose();
//        unload.Dispose();
//
//        _lastChunk = center;
//    }
//
//
//    private NativeArray<int3> CollectRequests(int3 center, int distance)
//    {
//        var realAxisSize = distance * 2 + 1;
//        var flatSize = realAxisSize * realAxisSize * realAxisSize;
//        var loading = new NativeArray<int3>(flatSize, Allocator.TempJob);
//
//        var counter = 0;
//        for (var x = -distance; x <= distance; x++)
//        for (var y = -distance; y <= distance; y++)
//        for (var z = -distance; z <= distance; z++)
//        {
//            loading[counter] = new int3(x, y, z) + center;
//            counter++;
//        }
//
//        return loading;
//    }
//
//
//    public int3 GetCurrentChunkPos()
//    {
//        var realPos = (float3) Target.position;
//        realPos /= Chunk.AxisSize;
//        return new int3((int) math.floor(realPos.x), (int) math.floor(realPos.y), (int) math.floor(realPos.z));
//    }
//}
//
//
//public struct GatherChunksToLoadV2 : IJobParallelFor
//{
//    public static JobHandle Create(NativeArray<int3> requests, NativeHashSet<int3> loaded, out NativeQueue<int3> toLoad,
//        JobHandle handle = default)
//    {
//        toLoad = new NativeQueue<int3>(Allocator.TempJob);
//
//        return new GatherChunksToLoadV2
//        {
//            Requests = requests,
//            ChunksLoaded = loaded,
//            ToLoad = toLoad.AsParallelWriter()
//        }.Schedule(requests.Length, 64, handle);
//    }
//
//    [ReadOnly] public NativeArray<int3> Requests;
//    [WriteOnly] private NativeQueue<int3>.ParallelWriter ToLoad;
//
//    [ReadOnly] public NativeHashSet<int3> ChunksLoaded;
//
//    public void Execute(int index)
//    {
//        var req = Requests[index];
//        if (!ChunksLoaded.Contains(req))
//            ToLoad.Enqueue(req);
//    }
//}
//
//public struct GatherChunksToUnloadV2 : IJobParallelFor
//{
//    public static JobHandle Create(NativeArray<int3> foci, NativeArray<int> distances, NativeArray<int3> loaded,
//        out NativeQueue<int3> shouldUnload, JobHandle handle = default)
//    {
//        shouldUnload = new NativeQueue<int3>(Allocator.TempJob);
//        return new GatherChunksToUnloadV2
//        {
//            FocalPoints = foci,
//            Distances = distances,
//            ChunksLoaded = loaded,
//            ShouldUnload = shouldUnload.AsParallelWriter()
//        }.Schedule(loaded.Length, 64, handle);
//    }
//
//    [NativeDisableParallelForRestriction] [ReadOnly]
//    public NativeArray<int3> FocalPoints;
//
//    [NativeDisableParallelForRestriction] [ReadOnly]
//    public NativeArray<int> Distances;
//
//    [ReadOnly] public NativeArray<int3> ChunksLoaded;
//
//    [WriteOnly] public NativeQueue<int3>.ParallelWriter ShouldUnload;
//
//    public void Execute(int index)
//    {
//        var chunk = ChunksLoaded[index];
//        var outOfFocus = false;
//        for (var i = 0; i < FocalPoints.Length; i++)
//        {
//            var focal = FocalPoints[i];
//            var dist = Distances[i];
//
//            var delta = math.abs(chunk - focal);
//            outOfFocus |= (delta.x > dist || delta.y > dist || delta.z > dist);
//        }
//
//        if (outOfFocus)
//            ShouldUnload.Enqueue(chunk);
//    }
//}
////
////public struct GatherChunksToLoadV2 : IJobParallelFor
////{
////    public static JobHandle Create(NativeArray<int3> requests, NativeArray<int> distances, NativeArray<int3> loaded,
////        out NativeArray<bool> shouldLoad, JobHandle handle = default)
////    {
////        shouldLoad = new NativeArray<bool>(requests.Length, Allocator.TempJob);
////        return new GatherChunksToUnloadV2
////        {
////            FocalPoints = requests,
////            Distances = distances,
////            ChunksLoaded = loaded,
////            ShouldUnload = shouldLoad
////        }.Schedule(requests.Length, 64, handle);
////    }
////
////    [NativeDisableParallelForRestriction] [ReadOnly]
////    public NativeArray<int3> FocalPoints;
////
////    [NativeDisableParallelForRestriction] [ReadOnly]
////    public NativeArray<int> Distances;
////
////    [ReadOnly] public NativeArray<int3> ChunksLoaded;
////
////    [WriteOnly] public NativeArray<bool> ShouldUnload;
////
////    public void Execute(int index)
////    {
////        var chunk = ChunksLoaded[index];
////        var outOfFocus = false;
////        for (var i = 0; i < FocalPoints.Length; i++)
////        {
////            var focal = FocalPoints[i];
////            var dist = Distances[i];
////
////            var delta = math.abs(chunk - focal);
////            outOfFocus |= (delta.x > dist || delta.y > dist || delta.z > dist);
////        }
////
////        ShouldUnload[index] = outOfFocus;
////    }
////}
//
//public struct GatherChunksToUnload : IJobParallelFor
//{
//    public static JobHandle Create(NativeArray<int3> requests, NativeArray<int3> loaded,
//        out NativeArray<bool> shouldUnload, JobHandle handle = default)
//    {
//        shouldUnload = new NativeArray<bool>(loaded.Length, Allocator.TempJob);
//        return new GatherChunksToUnload
//        {
//            ChunksRequested = requests,
//            ChunksLoaded = loaded,
//            ShouldUnload = shouldUnload
//        }.Schedule(loaded.Length, 64, handle);
//    }
//
//
//    [NativeDisableParallelForRestriction] [ReadOnly]
//    public NativeArray<int3> ChunksRequested;
//
//    [ReadOnly] public NativeArray<int3> ChunksLoaded;
//
//    [WriteOnly] public NativeArray<bool> ShouldUnload;
//
//    public void Execute(int index)
//    {
//        for (var i = 0; i < ChunksRequested.Length; i++)
//        {
//            if (!ChunksRequested[i].Equals(ChunksLoaded[index])) continue;
//            ShouldUnload[index] = true;
//            return;
//        }
//
//        ShouldUnload[index] = false;
//    }
//}