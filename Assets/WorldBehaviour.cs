using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class WorldBehaviour : MonoBehaviour
{
//    private NativeWorld _world;
    private Queue<int3> _chunksToLoad;
    private Dictionary<int3, GameObject> _chunkObjects;
    private Dictionary<int3, Chunk> _world;

    [SerializeField] private Material mat;
    [SerializeField] private int ChunkSize;

    private static GameObject CreateGameObject(int3 chunkPos, Mesh mesh, Material material)
    {
        var go = new GameObject($"Chunk {chunkPos}");
        go.transform.position = (float3) chunkPos * Chunk.AxisSize;
        var mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        var mr = go.AddComponent<MeshRenderer>();
        mr.material = material;
        var mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh;
        return go;
    }

    // Start is called before the first frame update
    void Awake()
    {
        _world = new Dictionary<int3, Chunk>();
        _chunksToLoad = new Queue<int3>();
        _chunkObjects = new Dictionary<int3, GameObject>();

        StartCoroutine(AsyncLoader());
        
        for (var x = -ChunkSize; x <= ChunkSize; x++)
        for (var y = -ChunkSize; y <= ChunkSize; y++)
        for (var z = -ChunkSize; z <= ChunkSize; z++)
            Load(new int3(x, y, z));
    }


    private void Load(int3 chunkPos)
    {
        if (!_world.ContainsKey(chunkPos))
        {
            var chunk = new Chunk();
            _world[chunkPos] = chunk;
            _chunksToLoad.Enqueue(chunkPos);
//            _world.Add(chunkPos, chunk);
//            var handle = RenderUtilV2.VisiblityPass(chunk);
//            var mesh = new Mesh();
//            RenderUtilV2.Render(chunk, mesh, handle);
//            _chunkObjects[chunkPos] = CreateGameObject(chunkPos, mesh, mat);
        }
    }

    private IEnumerator AsyncLoader()
    {
        bool Func() => (_chunksToLoad.Count > 0);
        while (true)
        {
            yield return new WaitUntil(Func);
            if (_chunksToLoad.Count > 0)
            {
                var pos = _chunksToLoad.Dequeue();
                var chunk = _world[pos];
                yield return StartCoroutine(AsyncRender(pos, chunk));
            }
        }
    }
    private IEnumerator AsyncRender(int3 chunkPos, Chunk chunk)
    {
        var handle = RenderUtilV2.VisiblityPass(chunk);
        while (!handle.IsCompleted)
            yield return null;
        var mesh = new Mesh();
        yield return StartCoroutine(RenderUtilV2.RenderAsync(chunk, mesh, handle));
        _chunkObjects[chunkPos] = CreateGameObject(chunkPos, mesh, mat);
    }

    private void OnDestroy()
    {
        foreach (var key in _world.Keys)
            _world[key].Dispose();
    }
}