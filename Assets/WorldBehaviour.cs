using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Jobs;
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
    [SerializeField] private ChunkGenArgs args;


    private static GameObject CreateGameObject(Transform parent, int3 chunkPos, Mesh mesh, Material material)
    {
        var go = new GameObject($"Chunk {chunkPos}");
        go.transform.parent = parent;
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
//        StartCoroutine(LoaderCoroutine());

        Task.Run(()=>AsyncLoader(gameObject));
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

    private IEnumerator LoaderCoroutine()
    {
        while (true)
        {
            if (_chunksToLoad.Count > 0)
            {
                var pos = _chunksToLoad.Dequeue();
                var chunk = World[pos];
                yield return StartCoroutine(RenderCoroutine(pos, chunk));
            }

            yield return null;
        }
    }

    private async void AsyncLoader(GameObject go, int millisecondTimestep = 10)
    {
        if (gameObject == null)
            await Task.Delay(millisecondTimestep * 10);
        //We assume that if the object is destroyed, that we should stop doing jobs
        while (gameObject != null)
        {
            if (_chunksToLoad.Count > 0)
            {
                var pos = _chunksToLoad.Dequeue();
                var chunk = World[pos];
                await AsyncRender(pos, chunk, millisecondTimestep);
            }

            await Task.Delay(millisecondTimestep);
        }
    }

    private IEnumerator RenderCoroutine(int3 chunkPos, Chunk chunk)
    {
        var genPass = RenderUtilV2.GenerationOctavePass(chunkPos, chunk, args);
        while (!genPass.IsCompleted)
            yield return null;
        var visPass = RenderUtilV2.VisiblityPass(chunk, genPass);
        while (!visPass.IsCompleted)
            yield return null;
        var mesh = new Mesh();
        yield return StartCoroutine(RenderUtilV2.RenderCoroutine(chunk, mesh, visPass));

        _chunkObjects[chunkPos] = CreateGameObject(transform, chunkPos, mesh, mat);
    }

    private async Task<JobHandle> AsyncRender(int3 chunkPos, Chunk chunk, int millisecondTimestep = 10)
    {
        var genPass = RenderUtilV2.GenerationOctavePass(chunkPos, chunk, args);
        while (!genPass.IsCompleted)
            await Task.Delay(millisecondTimestep); //Wait a fraction of a second
        var visPass = RenderUtilV2.VisiblityPass(chunk, genPass);
        while (!visPass.IsCompleted)
            await Task.Delay(millisecondTimestep); //Wait a fraction of a second
//            yield return null;
        var mesh = new Mesh();
        var renderHandle = await RenderUtilV2.RenderAsync(chunk, mesh, visPass, millisecondTimestep);
        _chunkObjects[chunkPos] = CreateGameObject(transform, chunkPos, mesh, mat);
        return renderHandle;
    }

    private void OnDestroy()
    {
        foreach (var key in World.Keys)
            World[key].Dispose();
    }
}