using System;
using System.Collections;
using System.Collections.Generic;
using StandardAssets.Characters.FirstPerson;
using StandardAssets.Characters.Physics;
using Unity.Mathematics;
using UnityEngine;
using UniVox;
using UniVox.Types;
using UniVox.Unity;

public class PlayerStreamer : MonoBehaviour
{
    [SerializeField] private int _deltaSize = 1;
    [SerializeField] private UniverseManager _universeManager;
    [SerializeField] private OpenCharacterController _openCharacterController;
    [SerializeField] private FirstPersonBrain _firstPersonBrain;
    [SerializeField] private int3 _currentChunk;
    [SerializeField] private int3 _prevChunk;
    [SerializeField] private bool _isLoaded;
    [SerializeField] private bool _isRendered;
    [SerializeField] private bool _isRequesting;
    [SerializeField] private Vector3 _holdPos;

    private void Awake()
    {
        _currentChunk = GetChunkIndex(transform.position);
        _prevChunk = _currentChunk + new int3(1);
        _openCharacterController = GetComponent<OpenCharacterController>();
        _firstPersonBrain = GetComponent<FirstPersonBrain>();
        _holdPos = transform.position;
    }

    private int3 GetChunkIndex(Vector3 position)
    {
        var worldPosition = UnivoxUtil.ToVoxelSpace(transform.position);
        var chunkPosition = UnivoxUtil.ToChunkPosition(worldPosition);
        return chunkPosition;
    }

    private void Update()
    {
        if (!_isLoaded || !_isRendered)
            transform.position = _holdPos;

        _currentChunk = GetChunkIndex(transform.position);
        if (!_currentChunk.Equals(_prevChunk) || !_isLoaded)
        {
            var worldId = 0;
            var chunkId = new ChunkIdentity(worldId, _currentChunk);
            _isLoaded = _universeManager.ChunkManager.TryGetChunkHandle(chunkId, out _);


            if (!_isRequesting)
            {
                _holdPos = transform.position;
                for (var x = -_deltaSize; x <= _deltaSize; x++)
                for (var y = -_deltaSize; y <= _deltaSize; y++)
                for (var z = -_deltaSize; z <= _deltaSize; z++)
                {
                    var delta = new int3(x, y, z);
                    _universeManager.ChunkManager.RequestChunkLoad(new ChunkIdentity(worldId, _currentChunk + delta));
                }

                _isRequesting = true;
            }

            _isRendered = false;
        }

        if (_isLoaded && _isRequesting) _isRequesting = false;

        if (_isLoaded && !_isRendered)
        {
            var worldId = 0;
            var chunkId = new ChunkIdentity(worldId, _currentChunk);
            _isRendered = _universeManager.ChunkMeshManager.IsRendered(chunkId);
        }

        _firstPersonBrain.enabled = _openCharacterController.enabled = _isLoaded && _isRendered;

        _prevChunk = _currentChunk;
    }

    private void LateUpdate()
    {
        if (!_isLoaded || !_isRendered)
            transform.position = _holdPos;
    }
}