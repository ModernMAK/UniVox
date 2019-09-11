//using System.Collections.Generic;
//using System.Linq;
//using DefaultNamespace;
//using Rendering;
//using Types;
//using Unity.Mathematics;
//using UnityEngine;
//
//
//public class WorldBehaviour : MonoBehaviour
//{
//    private ChunkEngine _ce;
//
//    private Queue<int3> _toLoad;
//    private Queue<int3> _toUnload;
//    [SerializeField] private ChunkGenArgs args;
//
////    [SerializeField] private int worldSize;
//
//    [SerializeField] private Material mat;
//
//
//    public IEnumerable<int3> Loaded => _ce.Loaded;
//    public int LoadedCount => _ce.LoadedCount;
//
//    // Start is called before the first frame update
//    private void Awake()
//    {
//        _ce = new ChunkEngine {ChunkMaterial = mat, GenerationArgs = args};
//
//
//        _toLoad = new Queue<int3>();
//        _toUnload = new Queue<int3>();
//    }
//
//    private void RemoveFromQueue<T>(Queue<T> queue, T removeItem)
//    {
//        var len = queue.Count;
//        var found = false;
//        for (var i = 0; i < len; i++)
//        {
//            var item = queue.Dequeue();
//            if (!found && item.Equals(removeItem))
//            {
//                found = true;
//            }
//            else queue.Enqueue(item);
//        }
//    }
//
//    public void RequestUnload(int3 pos)
//    {
//        if (_toLoad.Contains(pos))
//        {
//            RemoveFromQueue(_toLoad, pos);
//            _toUnload.Enqueue(pos);
//        }
//        else if (_ce.HasChunk(pos))
//        {
//            _toUnload.Enqueue(pos);
//        }
//    }
//
//    public void RequestLoad(int3 pos)
//    {
//        if (_toUnload.Contains(pos))
//        {
//            RemoveFromQueue(_toUnload, pos);
//            _toLoad.Enqueue(pos);
//        }
//        else if (!_ce.HasChunk(pos))
//        {
//            _toLoad.Enqueue(pos);
//        }
//
////        //TODO this check is neccessary, find out why
////        if (_ce.HasChunk(pos) || _toUnload.Contains(pos) || _toLoad.Contains(pos))
////            return;
////        _toLoad.Enqueue(pos);
//    }
//
//
//    private void UpdateLoad(int minimum, float ratio)
//    {
//        var ratioAdd = ratio * (_toLoad.Count + _toUnload.Count);
//        UpdateLoad(minimum + (int)ratioAdd);
//    }
//
//    private void UpdateLoad(int batchSize)
//    {
//        var passes = 0;
//        while (batchSize > 0)
//        {
//            if (_toLoad.Count > 0)
//            {
//                var pos = _toLoad.Dequeue();
//                _ce.Load(pos);
//                passes++;
//            }
//
//            if (_toUnload.Count > 0)
//            {
//                var pos = _toUnload.Dequeue();
//                _ce.Unload(pos);
//                passes++;
//            }
//
//            if (passes > 0)
//            {
//                batchSize -= passes;
//                passes = 0;
//            }
//            else
//            {
//                batchSize = 0;
//            }
//        }
//    }
//
//    private const float ExpectedDeltaTime = 1f / 60f;
//
//    private void Update()
//    {
////        var batchSize = (int)math.clamp(math.ceil(Time.deltaTime / ExpectedDeltaTime), 1, 64);
//
//        UpdateLoad(1,0.1f);
//        _ce.Update();
//    }
//
//
//    private void OnDestroy()
//    {
//        _ce.Dispose();
//    }
//}

