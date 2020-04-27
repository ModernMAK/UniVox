using System;
using System.IO;
using System.Text;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UniVox.MeshGen;
using UniVox.MeshGen.Utility;
using UniVox.Serialization;
using UniVox.Types.Native;
using UniVox.WorldGen;

namespace UniVox.Unity
{
    public class InDevVoxelSandboxMaster : MonoBehaviour
    {
        public string singletonGameObjectName;

        [SerializeField] private ChunkGameObjectManager chunkGameObjectManager;

        private GameObject _singleton;
        [SerializeField] private string _worldName;
        private VoxelUniverse _universe;
        [Range(0f, 1f)] public float Solidity;

        private AbstractGenerator<int3, VoxelChunk> _chunkGen;
        private BinarySerializer<VoxelChunk> _chunkSerializer;
        private MeshGeneratorProxy<RenderChunk> _greedyMeshGen;
        private MeshGeneratorProxy<RenderChunk> _naiveMeshGen;

        private RenderChunk ConvertToRender(VoxelChunk chunk, Allocator allocator)
        {
            var temp = new RenderChunk(chunk.ChunkSize, allocator, NativeArrayOptions.UninitializedMemory);
            temp.Identities.CopyFrom(chunk.Identities);
            VoxelRenderUtility.CalculateCulling(chunk.Active, temp.Culling, chunk.ChunkSize, new JobHandle())
                .Complete();
            return temp;
        }

        private void Save(byte worldId, int3 position, VoxelChunk chunk)
        {
            var fullDir = Path.Combine(InDevPathUtil.SaveDirectory, _worldName,
                InDevVoxelChunkStreamer.GetChunkFileName(worldId, position));
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
            var fullDir = Path.Combine(InDevPathUtil.SaveDirectory, _worldName,
                InDevVoxelChunkStreamer.GetChunkFileName(worldId, position));
            using (var file = File.Open(fullDir, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(file, Encoding.Unicode))
                {
                    return _chunkSerializer.Deserialize(reader);
                }
            }
        }

        private bool TryLoad(byte worldId, int3 position, out VoxelChunk chunk)
        {
            try
            {
                chunk = Load(worldId, position);
                return true;
            }
            catch (FileNotFoundException fnfe)
            {
                Debug.LogWarning(fnfe.Message);
                chunk = default;
                return false;
            }
        }


        private void Generate(byte worldId, int3 key, out Mesh[] meshes)
        {
            var greedyMesh = new Mesh() {name = $"{worldId}_{key}_G"};
            var naiveMesh = new Mesh() {name = $"{worldId}_{key}_N"};
            meshes = new Mesh[2] {greedyMesh, naiveMesh};
            VoxelChunk chunk;
            JobHandle depends = default;
            var shouldSave = false;
            if (!TryLoad(worldId, key, out chunk))
            {
                chunk = new VoxelChunk(new int3(32), Allocator.TempJob);
                depends = _chunkGen.Generate(key, chunk);
                shouldSave = true;
            }


            using (var render = ConvertToRender(chunk, Allocator.TempJob))
            {
                var meshArr = Mesh.AllocateWritableMeshData(2);
                depends = _greedyMeshGen.Generate(meshArr[0], render, depends);
                depends = _naiveMeshGen.Generate(meshArr[1], render, depends);
                using (var gBound = new NativeValue<Bounds>(Allocator.TempJob))
                using (var nBound = new NativeValue<Bounds>(Allocator.TempJob))
                {
                    depends = _greedyMeshGen.GenerateBound(meshArr[0], gBound, depends);
                    depends = _naiveMeshGen.GenerateBound(meshArr[1], nBound, depends);

                    depends.Complete();
                    Mesh.ApplyAndDisposeWritableMeshData(meshArr, meshes);
                    greedyMesh.bounds = gBound;
                    naiveMesh.bounds = nBound;
                }
            }

            if (shouldSave)
                Save(worldId, key, chunk);

            chunk.Dispose();
//        return greedyMesh;
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
            _naiveMeshGen = new NaiveMeshGeneratorProxy();
            _greedyMeshGen = new GreedyMeshGeneratorProxy();

            _universe = new VoxelUniverse();

            Generate(0, new int3(0), out var meshes);
        }
    }
}