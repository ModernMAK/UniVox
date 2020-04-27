﻿using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;
using UniVox.Types;

public class UniverseChunkGenerator : MonoBehaviour
{
    [SerializeField] private int _seed = 8675309;
    [Range(0f, 1f)] [SerializeField] private float _solidity = 1f;

    private VoxelChunkGenerator _generator;

    public void Awake()
    {
        _generator = new VoxelChunkGenerator()
        {
            Seed = _seed,
            Solidity = _solidity
        };
    }


    public JobHandle Generate(ChunkIdentity chunkId, VoxelChunk chunk)
    {
        return _generator.Generate(chunkId.Chunk * chunk.ChunkSize, chunk);
    }
}