using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UniVox.MeshGen;
using UniVox.MeshGen.Utility;
using UniVox.Types;
using UniVox.Types.Native;

namespace UniVox.Unity
{
    [RequireComponent(typeof(ChunkGameObjectManager))]
    public class ChunkMeshManager : MonoBehaviour
    {
        public void InitializeManager(UniverseManager manager) => UniverseManager = manager;
        public UniverseManager UniverseManager { get; private set; }


        private VoxelMeshGenerator<RenderChunk> _meshGenerator;
        private Dictionary<ChunkIdentity, Mesh[]> _meshTable;
        private Queue<Mesh[]> _cachedMeshes;

        private LinkedList<DataHandle<RenderRequest>> _request;
        private ChunkGameObjectManager _chunkGameObjectManager;
        [SerializeField]
        private Material _debugMaterial;
        
        
        private void Awake()
        {
            _chunkGameObjectManager = GetComponent<ChunkGameObjectManager>();
            _meshGenerator = new GreedyChunkMeshGenerator();
            _cachedMeshes = new Queue<Mesh[]>();
            _meshTable = new Dictionary<ChunkIdentity, Mesh[]>();
            _request = new LinkedList<DataHandle<RenderRequest>>();
        }

        private Mesh[] GetMeshArray(ChunkIdentity chunkIdentity)
        {
            if (_cachedMeshes.Count == 0)
            {
                var meshes = new[]
                {
                    new Mesh()
                    {
                        name =
                            $"Mesh World_{chunkIdentity.World} Chunk_{chunkIdentity.Chunk.x}_{chunkIdentity.Chunk.y}_{chunkIdentity.Chunk.z}"
                    },
                    new Mesh()
                    {
                        name =
                            $"Collider World_{chunkIdentity.World} Chunk_{chunkIdentity.Chunk.x}_{chunkIdentity.Chunk.y}_{chunkIdentity.Chunk.z}"
                    }
                };
                return meshes;
            }

            return _cachedMeshes.Dequeue();
        }


        [BurstCompile]
        private struct CopyArrayJob<T> : IJobParallelFor where T : struct
        {
            [ReadOnly] public NativeArray<T> Source;
            [WriteOnly] public NativeArray<T> Destenation;


            public void Execute(int index)
            {
                Destenation[index] = Source[index];
            }
        }

        [BurstCompile]
        private struct CalculateMatIdJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<byte> Identities;
            [WriteOnly] public NativeArray<int> MaterialIds;
            [ReadOnly] public NativeArray<int> IdentityToMaterial;

            private const int DefaultMaterialIdentity = -1;
            public void Execute(int index)
            {
                if (IdentityToMaterial.Length <= 0)
                {
                    //Specify Debug
                    MaterialIds[index] = DefaultMaterialIdentity;
                }
                else
                {
                    int id = Identities[index];
                    //Wrap id
                    if (IdentityToMaterial.Length <= id)
                    {
                        id %= IdentityToMaterial.Length;
                    }
                
                    MaterialIds[index] = id;                    
                }

            }
        }

        private JobHandle ConvertToRenderable(ChunkIdentity chunkId, VoxelChunk chunk, out RenderChunk renderChunk,
            JobHandle depends = new JobHandle()) => ConvertToSimpleRenderable(chunkId, chunk, out renderChunk, depends);

        private JobHandle ConvertToSimpleRenderable(ChunkIdentity chunkId, VoxelChunk chunk,
            out RenderChunk renderChunk,
            JobHandle depends = new JobHandle())
        {
            renderChunk = new RenderChunk(chunk.ChunkSize);
            var flatSize = chunk.Flags.Length;

            depends = new CopyArrayJob<byte>()
            {
                Source = chunk.Identities,
                Destenation = renderChunk.Identities
            }.Schedule(flatSize, 1024, depends);


            var lookup = GameData.Instance.GetBlockToMaterial(Allocator.TempJob);
            depends = new CalculateMatIdJob()
            {
                Identities = renderChunk.Identities,
                MaterialIds = renderChunk.MaterialIds,
                IdentityToMaterial = lookup
            }.Schedule(flatSize, 1024, depends);

            lookup.Dispose(depends);
            depends = VoxelRenderUtility.CalculateCullingNaive(chunk.Flags, renderChunk.Culling, chunk.ChunkSize,
                depends);
            return depends;
        }


        private JobHandle ConvertToAdvancedRenderable(ChunkIdentity chunkId, VoxelChunk chunk,
            out RenderChunk renderChunk,
            JobHandle depends = new JobHandle())
        {
            renderChunk = new RenderChunk(chunk.ChunkSize);
            chunk.Identities.CopyTo(renderChunk.Identities);

            var neighborhood = UniverseManager.ChunkManager.GetChunkNeighborhood(chunkId);
            var matIds = renderChunk.MaterialIds;

////        var rangePerMat = Mathf.CeilToInt((float) byte.MaxValue / _materials.Length);
//            for (var i = 0;
//                i < renderChunk.Identities.Length;
//                i++)
//            {
////                matIds[i] = renderChunk.Identities[i] % _materials.Length;
////            if (matIds[i] >= _materials.Length)
////                matIds[i] = _materials.Length-1;
//            }

            depends = VoxelRenderUtility.CalculateCullingAdvanced(renderChunk.Culling, neighborhood, depends);
            return depends;
        }

        private struct RenderRequest : IDisposable
        {
            public ChunkIdentity ChunkIdentity { get; set; }
            public NativeList<int> UniqueMaterials { get; set; }
            public NativeValue<Bounds> MeshBound { get; set; }
            public NativeValue<Bounds> ColliderBound { get; set; }

            public Mesh.MeshDataArray MeshDataArray { get; set; }

            public float3 WorldPosition { get; set; }

            public void Dispose()
            {
                UniqueMaterials.Dispose();
                MeshBound.Dispose();
                ColliderBound.Dispose();
            }

            public JobHandle Dispose(JobHandle depends)
            {
                depends = UniqueMaterials.Dispose(depends);
                depends = MeshBound.Dispose(depends);
                depends = ColliderBound.Dispose(depends);
                return depends;
            }
        }

        public void RequestRender(ChunkIdentity chunkId, PersistentDataHandle<VoxelChunk> chunk)
        {
            var depends = chunk.Handle;
            var meshArrayData = Mesh.AllocateWritableMeshData(2);
            var meshBound = new NativeValue<Bounds>(Allocator.TempJob);
            var colliderBound = new NativeValue<Bounds>(Allocator.TempJob);
            var uniqueMats = new NativeList<int>(Allocator.TempJob);
            depends = ConvertToRenderable(chunkId, chunk.Data, out var renderChunk, depends);
            depends = _meshGenerator.GenerateMesh(meshArrayData[0], meshBound, uniqueMats, renderChunk, depends);
            depends = _meshGenerator.GenerateCollider(meshArrayData[1], colliderBound, renderChunk, depends);
            depends = renderChunk.Dispose(depends);
//        depends = chunk.Dispose(depends);DOH! We dont want to dispose the chunk
            chunk.DependOn(depends);


            var request = new RenderRequest()
            {
                ChunkIdentity = chunkId,
                ColliderBound = colliderBound,
                MeshBound = meshBound,
                MeshDataArray = meshArrayData,
                UniqueMaterials = uniqueMats,
                WorldPosition = chunk.Data.ChunkSize * chunkId.Chunk
            };

            _request.AddLast(new DataHandle<RenderRequest>(request, depends));
        }

        public void RequestHide(ChunkIdentity chunkId)
        {
            if (_meshTable.TryGetValue(chunkId, out var meshes))
            {
                _chunkGameObjectManager.Hide(chunkId);
                _cachedMeshes.Enqueue(meshes);
                _meshTable.Remove(chunkId);
            }
        }

        private void Update()
        {
            ProcessRenderResults();
        }

        public void ProcessRenderResults()
        {
            var current = _request.First;
            while (current != null)
            {
                var next = current.Next;
                var handle = current.Value.Handle;
                var data = current.Value.Data;

                if (handle.IsCompleted)
                {
                    handle.Complete();
                    var meshes = GetMeshArray(data.ChunkIdentity);
                    Mesh.ApplyAndDisposeWritableMeshData(data.MeshDataArray, meshes,
                        MeshUpdateFlags.DontRecalculateBounds);
                    meshes[0].bounds = data.MeshBound;
                    meshes[1].bounds = data.ColliderBound;

                    var mats = GetMaterials(data.UniqueMaterials);
                    _chunkGameObjectManager.Render(data.ChunkIdentity, data.WorldPosition, meshes[0], mats, meshes[1]);
                    data.Dispose();
                    _meshTable[data.ChunkIdentity] = meshes;

                    _request.Remove(current);
                }

                current = next;
            }
        }

        private Material[] GetMaterials(NativeList<int> materialIds)
        {
            var materials = GameData.Instance.Materials;
            var mats = new Material[materialIds.Length];
            for (var i = 0; i < materialIds.Length; i++)
            {
                var matId = materialIds[i];
                if (matId < 0)
                {
                    mats[i] = _debugMaterial;
                }
                else
                    mats[i] = materials[matId];
            }

            return mats;
        }

        public bool IsRendered(ChunkIdentity chunkId)
        {
            return _meshTable.ContainsKey(chunkId);
        }
    }
}