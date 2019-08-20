using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class WorldBehaviour : MonoBehaviour
{
    private Dictionary<int3, GameObject> _chunkObjects;

//    private NativeWorld _world;
    private Queue<int3> _chunksToLoad;
    public Dictionary<int3, Chunk> World { get; private set; }
    [SerializeField] private int chunkSize;

    [SerializeField] private Material mat;
    [SerializeField] private int seed;
    [Range(0f, 1f)] [SerializeField] private float threshold;

    [SerializeField] private float freq;

    [SerializeField] private float res;

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
    private void Awake()
    {
        World = new Dictionary<int3, Chunk>();
        _chunksToLoad = new Queue<int3>();
        _chunkObjects = new Dictionary<int3, GameObject>();

        StartCoroutine(AsyncLoader());

        for (var x = -chunkSize; x <= chunkSize; x++)
        for (var y = -chunkSize; y <= chunkSize; y++)
        for (var z = -chunkSize; z <= chunkSize; z++)
            Load(new int3(x, y, z));
    }


    private void Load(int3 chunkPos)
    {
        if (!World.ContainsKey(chunkPos))
        {
            var chunk = new Chunk();
            World[chunkPos] = chunk;
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
        bool Func()
        {
            return _chunksToLoad.Count > 0;
        }

        while (true)
        {
            yield return new WaitUntil(Func);
            if (_chunksToLoad.Count > 0)
            {
                var pos = _chunksToLoad.Dequeue();
                var chunk = World[pos];
                yield return StartCoroutine(AsyncRender(pos, chunk));
            }
        }
    }

    private IEnumerator AsyncRender(int3 chunkPos, Chunk chunk)
    {
        var genPass = RenderUtilV2.GenerationPass(seed, chunkPos, chunk, freq, res, threshold);
        while (!genPass.IsCompleted)
            yield return null;
        var visPass = RenderUtilV2.VisiblityPass(chunk, genPass);
        while (!visPass.IsCompleted)
            yield return null;
        var mesh = new Mesh();
        yield return StartCoroutine(RenderUtilV2.RenderAsync(chunk, mesh, visPass));
        _chunkObjects[chunkPos] = CreateGameObject(chunkPos, mesh, mat);
    }

    private void OnDestroy()
    {
        foreach (var key in World.Keys)
            World[key].Dispose();
    }
}