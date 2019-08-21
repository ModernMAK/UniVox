using System;
using System.Collections.Generic;
using Types.Native;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


public class ChunkMeshData
{
    public ChunkMeshData(int3 pos, Chunk c)
    {
        Position = pos;
        Chunk = c;
    }

    public int3 Position { get; }
    public Chunk Chunk { get; }
}


[Obsolete]
public class ChunkManager : IDisposable
{
    private readonly Dictionary<int3, Chunk> _lookup;

    private readonly List<int3> _requestingLoad;
    private readonly List<int3> _requestingUnload;
    private readonly List<ChunkMeshData> _loaded;

    public ChunkManager()
    {
        _lookup = new Dictionary<int3, Chunk>();
        _requestingLoad = new List<int3>();
        _requestingUnload = new List<int3>();
        _loaded = new List<ChunkMeshData>();
    }

    public bool RequestLoad(int3 chunkPos)
    {
        if (!IsLoaded(chunkPos))
        {
            if (!_requestingLoad.Contains(chunkPos))
            {
                _requestingLoad.Add(chunkPos);
            }

            if (_requestingUnload.Contains(chunkPos))
            {
                _requestingUnload.Remove(chunkPos);
            }

            return true;
        }

        return false;
    }

    public bool IsLoaded(int3 chunkPos)
    {
        return _lookup.ContainsKey(chunkPos);
    }

    public bool TryGet(int3 chunkPos, out Chunk chunk)
    {
        return _lookup.TryGetValue(chunkPos, out chunk);
    }

    public IReadOnlyList<ChunkMeshData> GetLoadedThisUpdate() => _loaded;


    public bool RequestUnload(int3 chunkPos)
    {
        if (IsLoaded(chunkPos))
        {
            if (!_requestingUnload.Contains(chunkPos))
            {
                _requestingUnload.Add(chunkPos);
            }

            if (_requestingLoad.Contains(chunkPos))
            {
                _requestingLoad.Remove(chunkPos);
            }

            return true;
        }

        return false;
    }

    private void Load(int3 chunkPos)
    {
        Load(chunkPos, out _);
    }

    private void Load(int3 chunkPos, out Chunk chunk)
    {
        chunk = _lookup[chunkPos] = new Chunk();
    }

    private void Unload(int3 chunkPos)
    {
        _lookup[chunkPos].Dispose();
        _lookup.Remove(chunkPos);
    }

    public void Update(int batchSize = 1)
    {
        _loaded.Clear();
        while (batchSize > 0 && (_requestingLoad.Count > 0 || _requestingUnload.Count > 0))
        {
            if (_requestingLoad.Count > 0)
            {
                var pos = _requestingLoad[0];
                Load(pos, out var chunk);
                _requestingLoad.Remove(0);
                batchSize--;
                var data = new ChunkMeshData(pos, chunk);
                _loaded.Add(data);
            }

            if (_requestingUnload.Count > 0)
            {
                Unload(_requestingUnload[0]);
                _requestingUnload.Remove(0);
                batchSize--;
            }
        }
    }

    public void Dispose()
    {
        foreach (var key in _lookup.Keys)
            _lookup[key].Dispose();
    }
}