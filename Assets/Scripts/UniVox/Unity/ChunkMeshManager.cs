﻿using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UniVox.MeshGen;
using UniVox.MeshGen.Utility;
using UniVox.Types;
using UniVox.Types.Native;

[RequireComponent(typeof(ChunkGameObjectManager))]
public class ChunkMeshManager : MonoBehaviour
{
    private VoxelMeshGenerator<RenderChunk> _meshGenerator;
    private Dictionary<ChunkIdentity, Mesh[]> _meshTable;
    private Queue<Mesh[]> _cachedMeshes;
    [SerializeField] private Material[] _materials;

    private LinkedList<DataHandle<RenderRequest>> _request;
    private ChunkGameObjectManager _chunkGameObjectManager;

    private void Awake()
    {
        _chunkGameObjectManager = GetComponent<ChunkGameObjectManager>();
        _meshGenerator = new NaiveChunkMeshGenerator();
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


    private JobHandle ConvertToRenderable(VoxelChunk chunk, out RenderChunk renderChunk,
        JobHandle depends = new JobHandle())
    {
        renderChunk = new RenderChunk(chunk.ChunkSize);
        chunk.Identities.CopyTo(renderChunk.Identities);
        var matIds = renderChunk.MaterialIds;

        for (var i = 0; i < renderChunk.Identities.Length; i++)
        {
            matIds[i] = renderChunk.Identities[i] % _materials.Length;
        }

        depends = VoxelRenderUtility.CalculateCulling(chunk.Active, renderChunk.Culling, chunk.ChunkSize, depends);
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

    public void RequestRender(ChunkIdentity chunkId, VoxelChunk chunk)
    {
        var meshArrayData = Mesh.AllocateWritableMeshData(2);
        var meshBound = new NativeValue<Bounds>(Allocator.TempJob);
        var colliderBound = new NativeValue<Bounds>(Allocator.TempJob);
        var uniqueMats = new NativeList<int>(Allocator.TempJob);
        var depends = new JobHandle();
        depends = ConvertToRenderable(chunk, out var renderChunk, depends);
        depends = _meshGenerator.GenerateMesh(meshArrayData[0], meshBound, uniqueMats, renderChunk, depends);
        depends = _meshGenerator.GenerateCollider(meshArrayData[1], colliderBound, renderChunk, depends);
        depends = renderChunk.Dispose(depends);
//        depends = chunk.Dispose(depends);DOH! We dont want to dispose the chunk

        var request = new RenderRequest()
        {
            ChunkIdentity = chunkId,
            ColliderBound = colliderBound,
            MeshBound = meshBound,
            MeshDataArray = meshArrayData,
            UniqueMaterials = uniqueMats,
            WorldPosition = chunk.ChunkSize * chunkId.Chunk
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
                Mesh.ApplyAndDisposeWritableMeshData(data.MeshDataArray, meshes, MeshUpdateFlags.DontRecalculateBounds);
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
        var mats = new Material[materialIds.Length];
        for (var i = 0; i < materialIds.Length; i++)
        {
            mats[i] = _materials[materialIds[i]];
        }

        return mats;
    }
}