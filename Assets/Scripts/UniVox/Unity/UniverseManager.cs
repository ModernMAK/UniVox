using System;
using UnityEngine;
using UniVox.Types;

public class UniverseManager : MonoBehaviour
{
    [SerializeField] private UniverseChunkManager _chunkManager;
    [SerializeField] private ChunkMeshManager chunkMeshManager;


    public UniverseChunkManager ChunkManager => _chunkManager;
    public ChunkMeshManager ChunkMeshManager => chunkMeshManager;


    private void Awake()
    {
        if (_chunkManager == null)
            _chunkManager = GetComponentInChildren<UniverseChunkManager>();

        if (chunkMeshManager == null)
            chunkMeshManager = GetComponentInChildren<ChunkMeshManager>();
    }

    private void OnEnable()
    {
        ChunkManager.ChunkLoaded += ChunkLoaded;
        ChunkManager.ChunkUnloaded += ChunkUnloaded;
    }
    private void OnDisable()
    {
        ChunkManager.ChunkLoaded -= ChunkLoaded;
        ChunkManager.ChunkUnloaded -= ChunkUnloaded;
    }

    private void ChunkLoaded(object sender, ChunkLoadedArgs args)
    {
        ChunkMeshManager.RequestRender(args.Identity, args.Chunk);
    }
    private void ChunkUnloaded(object sender, ChunkIdentity args)
    {
        ChunkMeshManager.RequestHide(args);
    }
}