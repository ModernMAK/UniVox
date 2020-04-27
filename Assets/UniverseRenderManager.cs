using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class UniverseRenderManager : MonoBehaviour
{
    private struct ChunkObject
    {
        public ChunkObject(Transform transform)
        {
            Transform = transform;
            Collider = transform.GetComponent<MeshCollider>();
            Filter = transform.GetComponent<MeshFilter>();
            Renderer = transform.GetComponent<MeshRenderer>();
        }

        public Transform Transform { get; }
        public MeshCollider Collider { get; }
        public MeshFilter Filter { get; }
        public MeshRenderer Renderer { get; }
    }

    [SerializeField] private GameObject _templateChunk;
    [SerializeField] private Transform _cachedChunkContainer;
    [SerializeField] private Transform _cachedContainerContainer;
    [SerializeField] private Transform _worldContainer;

    private Dictionary<int, Transform> _worldTable;
    private Dictionary<int4, ChunkObject> _chunkTable;

    private void Awake()
    {
        _worldTable = new Dictionary<int, Transform>();
        _chunkTable = new Dictionary<int4, ChunkObject>();
    }

    private void CacheContainer(Transform container)
    {
        container.parent = _cachedContainerContainer;
        container.gameObject.SetActive(false);
    }

    private void CacheChunk(Transform chunk)
    {
        if (chunk.parent.childCount == 1)
            CacheContainer(chunk.parent);
        chunk.parent = _cachedChunkContainer;
        chunk.gameObject.SetActive(false);
    }


    public void Render(int worldId, int3 chunkPos, int3 worldPos, Mesh mesh, Material[] materials, Mesh meshCollider)
    {
        if (mesh.subMeshCount != materials.Length)
            throw new Exception("Too Many / Not Enough Materials");

        var mergedPos = new int4(chunkPos, worldId);
        if (!_worldTable.TryGetValue(worldId, out var container))
        {
            container = GetContainer();
            container.name = $"World {worldId}";
            container.parent = _worldContainer;
            _worldTable[worldId] = container;
        }

        if (!_chunkTable.TryGetValue(mergedPos, out var chunk))
        {
            chunk = GetChunkObject();
            chunk.Transform.parent = container;
            _chunkTable[mergedPos] = chunk;
        }

        chunk.Transform.name = $"Chunk_{chunkPos.x}_{chunkPos.y}_{chunkPos.z}";
        chunk.Filter.mesh = mesh;
        chunk.Renderer.materials = materials;
        chunk.Collider.sharedMesh = meshCollider;
        chunk.Transform.position = (float3) worldPos;
        chunk.Transform.gameObject.SetActive(true);
    }


    private Transform GetContainer()
    {
        if (_cachedContainerContainer.childCount > 0)
        {
            var cached = _cachedContainerContainer.GetChild(0);
            cached.parent = null;
            return cached;
        }
        else
        {
            return new GameObject().transform;
        }
    }

    private Transform GetChunk()
    {
        if (_cachedChunkContainer.childCount > 0)
        {
            var cached = _cachedChunkContainer.GetChild(0);
            cached.parent = null;
            return cached;
        }
        else
        {
            return Instantiate(_templateChunk).transform;
        }
    }

    private ChunkObject GetChunkObject()
    {
        return new ChunkObject(GetChunk());
    }
}