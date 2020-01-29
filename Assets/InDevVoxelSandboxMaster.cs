using System;
using System.IO;
using System.Text;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Rendering;
using UniVox.Types.Native;
using Random = Unity.Mathematics.Random;

public class InDevVoxelSandboxMaster : MonoBehaviour
{
    public string singletonGameObjectName;

    private GameObject _singleton;
    private string _worldName;
    private VoxelUniverse _universe;
    [Range(0f, 1f)] public float Solidity;

    private AbstractGenerator<int3, VoxelChunk> _chunkGen;
    private BinarySerializer<VoxelChunk> _chunkSerializer;
    private MeshGeneratorProxy<RenderChunk> _meshGen;

    private RenderChunk ConvertToRender(VoxelChunk chunk, Allocator allocator)
    {
        var temp = new RenderChunk(chunk.ChunkSize, allocator, NativeArrayOptions.UninitializedMemory);
        temp.Identities.CopyFrom(chunk.Identities);
        VoxelRenderUtility.CalculateCulling(chunk.Active, temp.Culling, chunk.ChunkSize, new JobHandle()).Complete();
        return temp;
    }

    private void Save(byte worldId, int3 position, VoxelChunk chunk)
    {
        var fullDir = Path.Combine(InDevPathUtil.WorldDirectory, _worldName,
            InDevVoxelSerializer.GetChunkFileName(worldId, position));
        using (var file = File.Open(fullDir, FileMode.Create, FileAccess.Write))
        {
            using (var writer = new BinaryWriter(file, Encoding.Unicode))
            {
                _chunkSerializer.Serialize(writer, chunk);
            }
        }
    }

    private VoxelChunk Load(byte worldId, int3 position)
    {
        var fullDir = Path.Combine(InDevPathUtil.WorldDirectory, _worldName,
            InDevVoxelSerializer.GetChunkFileName(worldId, position));
        using (var file = File.Open(fullDir, FileMode.Open, FileAccess.Read))
        {
            using (var reader = new BinaryReader(file, Encoding.Unicode))
            {
                return _chunkSerializer.Deserialize(reader);
            }
        }
    }


    private Mesh Generate(byte worldId, int3 key)
    {
        using (var chunk = new VoxelChunk(new int3(32), Allocator.TempJob))
        {
            _chunkGen.Generate(key, chunk);

            Save(worldId, key, chunk);

            using (var render = ConvertToRender(chunk, Allocator.TempJob))
            {
                var meshArr = Mesh.AllocateWritableMeshData(1);
                _meshGen.Generate(meshArr[0], render, new JobHandle()).Complete();
                using (var bound = new NativeValue<Bounds>(Allocator.TempJob))
                {
                    _meshGen.GenerateBound(meshArr[0], bound, new JobHandle()).Complete();

                    var mesh = new Mesh() {name = $"{worldId}_{key}"};
                    Mesh.ApplyAndDisposeWritableMeshData(meshArr, mesh);
                    mesh.bounds = bound;
                    return mesh;
                }
            }
        }
    }

    void Awake()
    {
        _singleton = GameObject.Find(singletonGameObjectName);
        if (_singleton == null)
            throw new NullReferenceException($"Singleton '{singletonGameObjectName}' not found!");

        var wi = _singleton.GetComponent<InDevWorldInformation>();
        _worldName = wi.WorldName;

        var seed = _worldName.GetHashCode();
        _chunkGen = new VoxelChunkGenerator() {Seed = seed, Solidity = Solidity};
        _chunkSerializer = new ChunkSerializer();
        _meshGen = new NaiveMeshGeneratorProxy();

        _universe = new VoxelUniverse();

        var m = Generate(0, new int3(0));
        gameObject.AddComponent<MeshFilter>();

        GetComponent<MeshFilter>().mesh = m;
    }


    public struct RandomBoolJob : IJob
    {
        public Random Rand;
        public NativeArray<bool> Array;


        public void Execute()
        {
            for (var i = 0; i < Array.Length; i++)
                Array[i] = Rand.NextBool();
        }
    }

    public struct RandomByteJob : IJob
    {
        public Random Rand;
        public NativeArray<byte> Array;


        public void Execute()
        {
            for (var i = 0; i < Array.Length; i++)
                Array[i] = (byte) Rand.NextInt(0, byte.MaxValue + 1);
        }
    }
}