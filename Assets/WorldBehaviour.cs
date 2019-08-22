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


    private Queue<GoData> Pool;
    private ChunkTableManager _cm;
    private ChunkTableManager _icm;
    private GenerationPipelineV2 _cgp;
    private ChunkRenderPipelineV2 _vrp;
    private Dictionary<int3, Mesh> _meshes;

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

        public bool Enabled
        {
            get => GO.activeSelf;
            set => GO.SetActive(value);
        }

        public void ResetMesh()
        {
            Mesh = Mesh;
        }
    }

    private static GoData CreateGameObject(Transform parent, int3 chunkPos)
    {
        var go = new GameObject($"Chunk {chunkPos}");
        UpdatePos(go, parent, chunkPos);
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        var mc = go.AddComponent<MeshCollider>();
        return new GoData(go, mf, mr, mc);
    }

    private static void UpdatePos(GameObject go, Transform parent, int3 chunkPos)
    {
        go.transform.parent = parent;
        go.transform.position = (float3) chunkPos * Chunk.AxisSize;
    }

    private static void UpdateMeshMat(GoData data, Mesh mesh, Material material)
    {
        data.Mesh = mesh;
        data.Mat = material;
    }

    private static GoData CreateGameObject(Transform parent, int3 chunkPos, Mesh mesh, Material material)
    {
        var data = CreateGameObject(parent, chunkPos);
        UpdateMeshMat(data, mesh, material);
        return data;
    }

    private GoData CreateGameObjectFromPool(Transform parent, int3 chunkPos)
    {
        if (Pool.Count > 0)
        {
            var go = Pool.Dequeue();
            UpdatePos(go.GO, parent, chunkPos);
            return go;
        }

        return CreateGameObject(parent, chunkPos);
    }

    private GoData CreateGameObjectFromPool(Transform parent, int3 chunkPos, Mesh mesh, Material material)
    {
        if (Pool.Count > 0)
        {
            var go = Pool.Dequeue();
            UpdatePos(go.GO, parent, chunkPos);
            UpdateMeshMat(go, mesh, mat);
            return go;
        }

        return CreateGameObject(parent, chunkPos, mesh, material);
    }

    // Start is called before the first frame update
    private void Awake()
    {
        _cm = new ChunkTableManager();
        _icm = new ChunkTableManager();
        _chunkObjects = new Dictionary<int3, GoData>();

        _vrp = new ChunkRenderPipelineV2();
        _vrp.Completed += VrpOnCompleted;

        _cgp = new GenerationPipelineV2();
        _cgp.Completed += CgpOnCompleted;

        _toLoad = new Queue<int3>();
        _toUnload = new Queue<int3>();

        Pool = new Queue<GoData>();

        _meshes = new Dictionary<int3, Mesh>();
//
//        for (var x = -worldSize; x <= worldSize; x++)
//        for (var y = -worldSize; y <= worldSize; y++)
//        for (var z = -worldSize; z <= worldSize; z++)
//            _toLoad.Enqueue(new int3(x, y, z));
    }

    private void CgpOnCompleted(object sender, int3 e)
    {
        AddToManagerAndRender(e);
    }

    private void VrpOnCompleted(object sender, int3 e)
    {
        CreateMeshRenderer(e);
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
        if (_icm.IsLoaded(pos) || _cm.IsLoaded(pos))
            return;
        _toLoad.Enqueue(pos);
    }

    public void Load(int3 pos)
    {
        var c = new Chunk();
        _icm.Load(pos, c);
        _cgp.AddJob(pos, c, args);
    }

    public void Unload(int3 pos)
    {
        _cgp.RemoveJob(pos);

        _vrp.RemoveJob(pos);


        _icm.Unload(pos);
        _cm.Unload(pos);

        _meshes.Remove(pos);

        if (_chunkObjects.TryGetValue(pos, out var data))
        {
            data.Enabled = false;
            Pool.Enqueue(data);
            _chunkObjects.Remove(pos);
        }
    }

    public bool IsLoaded(int3 pos) => _cm.IsLoaded(pos);

    public IEnumerable<int3> Loaded => _cm.Loaded;
    public int LoadedCount => _cm.LoadedCount;

    private void AddToManagerAndRender(int3 position)
    {
        var chunk = _icm.Get(position);
        _icm.TransferTo(position, _cm);
        var mesh = _meshes[position] = new Mesh();
        _vrp.AddJob(position, chunk, mesh);
//            CreateMeshRenderer(position, mesh)
    }


    private void UpdateLoad(int batchSize)
    {
        int passes = 0;
        while (batchSize > 0)
        {
            if (_toLoad.Count > 0)
            {
                var pos = _toLoad.Dequeue();
                Load(pos);
                passes++;
            }

            if (_toUnload.Count > 0)
            {
                var pos = _toUnload.Dequeue();
                Unload(pos);
                passes++;
            }

            if (passes > 0)
            {
                batchSize -= passes;
                passes = 0;
            }
            else batchSize = 0;
        }
    }

    private void Update()
    {
        UpdateLoad(8);
        _cgp.UpdateEvents();
        _vrp.UpdateEvents();
    }


    private void CreateMeshRenderer(int3 chunkPos)
    {
        if (_chunkObjects.ContainsKey(chunkPos))
        {
            _chunkObjects[chunkPos].ResetMesh();
        }
        else
        {
            _chunkObjects[chunkPos] = CreateGameObjectFromPool(transform, chunkPos, _meshes[chunkPos], mat);
        }
    }

    private void OnDestroy()
    {
        _cgp.Dispose();
        _vrp.Dispose();
        _cm.Dispose();
        _icm.Dispose();
    }
}