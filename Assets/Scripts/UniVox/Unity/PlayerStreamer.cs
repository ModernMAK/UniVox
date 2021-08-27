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
    //[SerializeField] private OpenCharacterController _openCharacterController;
    [SerializeField] private FirstPersonBrain _firstPersonBrain;
    [SerializeField] private int3 _currentChunk;
    [SerializeField] private int3 _prevChunk;
    [SerializeField] private ChunkState _currentChunkState;

    //[SerializeField] private bool _isLoaded;
    //[SerializeField] private bool _isRendered;
    //[SerializeField] private bool _isRequesting;

    [SerializeField] private Vector3 _holdPos;
    // Should probably be an actual field I can check for a chunk's state. 
    //TODO: Allow UniverseManager to poll a chunk's state
    public enum ChunkState 
    {
        Unknown = -1,
        Requesting, // May or may not exist yet
        Loaded, // Loaded but may not be rendered ~ No collision Data
        Rendered, // Loaded & Rendered, ~ No Collision Data
        Valid // Loaded ~ Collision Data Present
    }

    private void Awake()
    {
        _currentChunkState = ChunkState.Unknown;
        _currentChunk = GetChunkIndex(transform.position);
        _prevChunk = _currentChunk + new int3(1);
        //_openCharacterController = GetComponent<OpenCharacterController>();
        _firstPersonBrain = GetComponent<FirstPersonBrain>();
        _holdPos = transform.position;
    }

    private int3 GetChunkIndex(Vector3 position)
    {
        var worldPosition = UnivoxUtil.ToVoxelSpace(transform.position);
        var chunkPosition = UnivoxUtil.ToChunkPosition(worldPosition, _universeManager.ChunkManager.ChunkSize);
        return chunkPosition;
    }

    private void ChunkChanged(ChunkIdentity chunkId)
    {
        var worldId = chunkId.World;
        //Always update hold pos and previous chunk
        _holdPos = transform.position;
        _prevChunk = _currentChunk;
        //Always request neighbors  

        for (var x = -_deltaSize; x <= _deltaSize; x++)
            for (var y = -_deltaSize; y <= _deltaSize; y++)
                for (var z = -_deltaSize; z <= _deltaSize; z++)
                {
                    var delta = new int3(x, y, z);
                    _universeManager.ChunkManager.RequestChunkLoad(new ChunkIdentity(worldId, _currentChunk + delta));
                }

        if (_universeManager.ChunkGameObjectManager.IsCreated(chunkId))
            _currentChunkState = ChunkState.Valid;
        else if (_universeManager.ChunkMeshManager.IsRendered(chunkId))
            _currentChunkState = ChunkState.Rendered;
        else if (_universeManager.ChunkManager.TryGetChunkHandle(chunkId, out _))
            _currentChunkState = ChunkState.Loaded;
        else
            _currentChunkState = ChunkState.Requesting;
    }

    private void Update()
    {
        _currentChunk = GetChunkIndex(transform.position);
        //Debug.Log($"{transform.position} => {_currentChunk} => {_currentChunkState}");
        var worldId = 0;
        var chunkId = new ChunkIdentity(worldId, _currentChunk);
        if (!_currentChunk.Equals(_prevChunk)) // Changed Chunk Positions
        {
            ChunkChanged(chunkId);
        }
        else if(_currentChunkState != ChunkState.Valid)
        {
            switch (_currentChunkState)
            {
                case ChunkState.Unknown: // Safety case; this shouldn't ever happen, but if we do end up here; this will get us back on track
                    _holdPos = transform.position;
                    for (var x = -_deltaSize; x <= _deltaSize; x++)
                        for (var y = -_deltaSize; y <= _deltaSize; y++)
                            for (var z = -_deltaSize; z <= _deltaSize; z++)
                            {
                                var delta = new int3(x, y, z);
                                _universeManager.ChunkManager.RequestChunkLoad(new ChunkIdentity(worldId, _currentChunk + delta));
                            }
                    _currentChunkState = ChunkState.Requesting;
                    break;
                case ChunkState.Requesting:
                    _holdPos = transform.position;
                    if (_universeManager.ChunkManager.TryGetChunkHandle(chunkId, out _))
                        _currentChunkState = ChunkState.Loaded;
                    break;
                case ChunkState.Loaded:
                    _holdPos = transform.position;
                    if (_universeManager.ChunkMeshManager.IsRendered(chunkId))
                        _currentChunkState = ChunkState.Rendered;
                    break;
                case ChunkState.Rendered:
                    _holdPos = transform.position;
                    if (_universeManager.ChunkGameObjectManager.IsCreated(chunkId))
                        _currentChunkState = ChunkState.Valid;
                    break;
                case ChunkState.Valid:
                default:
                    // Shouldn't ever reach this case
                    break;
            }

        }

        _firstPersonBrain.enabled = (_currentChunkState == ChunkState.Valid);
        _prevChunk = _currentChunk;
    }

    private void LateUpdate()
    {
        if (_currentChunkState != ChunkState.Valid)
            transform.position = _holdPos;
    }
}