using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UniVox.MeshGen;
using UniVox.MeshGen.Utility;
using UniVox.Types;
using UniVox.Types.Native;

public class IndevUniverseManager : MonoBehaviour
{
    [SerializeField] private ChunkGameObjectManager _chunkGameObjectManager;
    private NaiveChunkMeshGenerator _naiveChunkMeshGen;
    private VoxelChunkGenerator _voxelChunkGenerator;
    
    [SerializeField] private bool _runMeshGen;
    [SerializeField] private Material[] _materials;
    [SerializeField] private int _seed;
    
    [Range(0f, 1f)] [SerializeField] private float _solidity;
    [SerializeField] private int3 _testChunkRange;
    [SerializeField] private int3 _testChunkSize;

    private void Awake()
    {
        _naiveChunkMeshGen = new NaiveChunkMeshGenerator();
        _voxelChunkGenerator = new VoxelChunkGenerator()
        {
            Seed = _seed,
            Solidity = _solidity
        };
    }

    private void TestMeshGen(int3 chunkPos, int3 chunkSize)
    {
        var worldPos = chunkPos * chunkSize;
        var chunk = new VoxelChunk(chunkSize);
        var depends = new JobHandle();
        depends = _voxelChunkGenerator.Generate(worldPos, chunk, depends);
        var meshArrayData = Mesh.AllocateWritableMeshData(2);
        var meshBound = new NativeValue<Bounds>(Allocator.TempJob);
        var colliderBound = new NativeValue<Bounds>(Allocator.TempJob);
        var uniqueMats = new NativeList<int>(Allocator.TempJob);
        depends.Complete();
        depends = ConvertToRenderable(chunk, out var renderChunk, depends);
        depends = _naiveChunkMeshGen.GenerateMesh(meshArrayData[0], meshBound, uniqueMats, renderChunk, depends);
        depends = _naiveChunkMeshGen.GenerateCollider(meshArrayData[1], colliderBound, renderChunk, depends);
        depends.Complete();

        var renderMesh = new Mesh()
        {
            name = $"Mesh World_{0} Chunk_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}"
        };
        var colliderMesh = new Mesh()
        {
            name = $"Collider World_{0} Chunk_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}"
        };

        Mesh.ApplyAndDisposeWritableMeshData(meshArrayData, new[] {renderMesh, colliderMesh},
            MeshUpdateFlags.DontRecalculateBounds);
        renderMesh.bounds = meshBound;
        colliderMesh.bounds = colliderBound;

        var mats = GetMaterials(uniqueMats);
        uniqueMats.Dispose();
        _chunkGameObjectManager.Render(new ChunkIdentity(0, chunkPos), worldPos, renderMesh, mats, colliderMesh);
        
        renderChunk.Dispose();
        chunk.Dispose();
        meshBound.Dispose();
        colliderBound.Dispose();
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


    private void Update()
    {
        if (_runMeshGen)
        {
            _runMeshGen = false;
            for (var x = -_testChunkRange.x; x <= _testChunkRange.x; x++)
            for (var y = -_testChunkRange.y; y <= _testChunkRange.y; y++)
            for (var z = -_testChunkRange.z; z <= _testChunkRange.z; z++)
                TestMeshGen(new int3(x,y,z), _testChunkSize);
        }
    }
}