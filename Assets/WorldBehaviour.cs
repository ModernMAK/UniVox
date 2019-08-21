using System;
using System.Collections.Generic;
using DefaultNamespace;
using Rendering;
using Types;
using Unity.Mathematics;
using UnityEngine;


public class WorldBehaviour : MonoBehaviour
{
    private Dictionary<int3, GoData> _chunkObjects;

//    [SerializeField] private int worldSize;

    [SerializeField] private Material mat;
    [SerializeField] private ChunkGenArgs args;

    private ChunkTableManager _cm;
    private GenerationPipeline _cgp;
    private Dictionary<int3, Chunk> _invalidBuffer;
    private ChunkRenderPipeline _vrp;

    private class GoData
    {
        public GoData(GameObject go, MeshFilter mf, MeshRenderer mr, MeshCollider mc)
        {
            GO = go;
            MF = mf;
            MR = mr;
            MC = mc;
        }

        public GameObject GO { get; }
        public MeshFilter MF { get; }
        public MeshRenderer MR { get; }
        public MeshCollider MC { get; }

        public Mesh Mesh
        {
            get => MF.mesh;
            set
            {
                MF.mesh = value;
                MC.sharedMesh = value;
            }
        }

        public Material Mat
        {
            get => MR.material;
            set => MR.material = value;
        }

        public void ResetMesh()
        {
            Mesh = Mesh;
        }
    }

    private static GoData CreateGameObject(Transform parent, int3 chunkPos)
    {
        var go = new GameObject($"Chunk {chunkPos}");
        go.transform.parent = parent;
        go.transform.position = (float3) chunkPos * Chunk.AxisSize;
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        var mc = go.AddComponent<MeshCollider>();
        return new GoData(go, mf, mr, mc);
    }

    private static GoData CreateGameObject(Transform parent, int3 chunkPos, Mesh mesh, Material material)
    {
        var data = CreateGameObject(parent, chunkPos);
        data.Mesh = mesh;
        data.Mat = material;
        return data;
    }

    // Start is called before the first frame update
    private void Awake()
    {
        _cm = new ChunkTableManager();
        _chunkObjects = new Dictionary<int3, GoData>();
        _vrp = new ChunkRenderPipeline();
        _invalidBuffer = new Dictionary<int3, Chunk>();
        _cgp = new GenerationPipeline();
        _toLoad = new Queue<int3>();
        _toUnload = new Queue<int3>();
//
//        for (var x = -worldSize; x <= worldSize; x++)
//        for (var y = -worldSize; y <= worldSize; y++)
//        for (var z = -worldSize; z <= worldSize; z++)
//            _toLoad.Enqueue(new int3(x, y, z));
    }

    private Queue<int3> _toLoad;
    private Queue<int3> _toUnload;

    public void RequestUnload(int3 pos)
    {
        _toUnload.Enqueue(pos);
    }

    public void RequestLoad(int3 pos)
    {
        //TODO this check is neccessary, find out why
        if (_invalidBuffer.ContainsKey(pos) || _cm.IsLoaded(pos))
            return;
        _toLoad.Enqueue(pos);
    }

    public void Load(int3 pos)
    {

        var c = new Chunk();
        _invalidBuffer.Add(pos, c);
        _cgp.RequestGeneration(pos, c, args, AddToManagerAndRender(pos, c));
    }

    public void Unload(int3 pos)
    {
        if (_cgp.TryGetHandle(pos, out var cgpHandle))
        {
            cgpHandle.Complete();
            cgpHandle.Dispose();
        }

        if (_vrp.TryGetHandle(pos, out var vrpHandle))
        {
            vrpHandle.Complete();
            vrpHandle.Dispose();
        }

        if (_invalidBuffer.TryGetValue(pos, out var c))
        {
            c.Dispose();
        }

        _cm.Unload(pos);
        if (_chunkObjects.TryGetValue(pos, out var data))
        {
            Destroy(data.GO);
            _chunkObjects.Remove(pos);
        }
    }

    public bool IsLoaded(int3 pos) => _cm.IsLoaded(pos);

    public IEnumerable<int3> Loaded => _cm.Loaded;
    public int LoadedCount => _cm.LoadedCount;

    private Action AddToManagerAndRender(int3 position, Chunk chunk)
    {
        return () =>
        {
            _cm.Load(position, chunk);
            _invalidBuffer.Remove(position);
            var mesh = new Mesh();
            _vrp.RequestRender(position, chunk, mesh, CreateMeshRenderer(position, mesh));
        };
    }

    private void Update()
    {
        if (_toLoad.Count > 0)
        {
            var pos = _toLoad.Dequeue();
            Load(pos);
        }

        if (_toUnload.Count > 0)
        {
            var pos = _toUnload.Dequeue();
            Unload(pos);
        }

        _cgp.Update();
        _vrp.Update();
    }


    private Action CreateMeshRenderer(int3 chunkPos, Mesh mesh)
    {
        return () =>
        {
            if (_chunkObjects.ContainsKey(chunkPos))
            {
                _chunkObjects[chunkPos].ResetMesh();
            }
            else
            {
                _chunkObjects[chunkPos] = CreateGameObject(transform, chunkPos, mesh, mat);
            }
        };
    }


    private void OnDestroy()
    {
        _cgp.Dispose();
        _vrp.Dispose();
        _cm.Dispose();
        foreach (var chunk in _invalidBuffer)
            chunk.Value.Dispose();
    }
}