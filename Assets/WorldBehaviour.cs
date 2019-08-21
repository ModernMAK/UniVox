using System;
using System.Collections.Generic;
using Rendering;
using Types;
using Unity.Mathematics;
using UnityEngine;


public class WorldBehaviour : MonoBehaviour
{
    private Dictionary<int3, GoData> _chunkObjects;

    [SerializeField] private int worldSize;

    [SerializeField] private Material mat;
    [SerializeField] private ChunkGenArgs args;

    private ChunkTableManager _cm;

    private VoxelRenderPipeline _vrp;

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
//        World = new Dictionary<int3, Chunk>();
        _cm = new ChunkTableManager();
//        _crm = new ChunkRenderManager();
        _chunkObjects = new Dictionary<int3, GoData>();
        _vrp = new VoxelRenderPipeline();
//        StartCoroutine(LoaderCoroutine());
        _toLoad = new Queue<int3>();

//        Task.Run(() => AsyncLoader(gameObject));
        for (var x = -worldSize; x <= worldSize; x++)
        for (var y = -worldSize; y <= worldSize; y++)
        for (var z = -worldSize; z <= worldSize; z++)
            _toLoad.Enqueue(new int3(x,y,z));
    }

    private Queue<int3> _toLoad;

    private Chunk Build(int3 pos)
    {
        var c = new Chunk();
        _cm.Load(pos, c);
        var handle = RenderUtilV2.GenerationOctavePass(pos, c, args);
        handle = RenderUtilV2.VisiblityPass(c, handle);
        handle.Complete();
        return c;
    }

    private void Update()
    {
        if (_toLoad.Count > 0)
        {
            var pos = _toLoad.Dequeue();
            var c = Build(pos);

            var mesh = new Mesh();
            _vrp.RequestRender(c, mesh, CreateActionCmd(pos, mesh));
        }

        _vrp.Update();

//        CreateCMD(_crm.GetRenderedThisUpdate());
    }


    private Action CreateActionCmd(int3 chunkPos, Mesh mesh)
    {
        return () => CreateCmd(chunkPos, mesh);
    }

    private void CreateCmd(int3 chunkPos, Mesh mesh)
    {
        if (_chunkObjects.ContainsKey(chunkPos))
        {
            _chunkObjects[chunkPos].ResetMesh();
        }
        else
        {
            _chunkObjects[chunkPos] = CreateGameObject(transform, chunkPos, mesh, mat);
        }
    }


    private void OnDestroy()
    {
        _cm.Dispose();
        _vrp.Dispose();
    }
}