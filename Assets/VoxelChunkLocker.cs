using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voxel;
using Voxel.Unity;

[RequireComponent(typeof(CharacterController))]
public class VoxelChunkLocker : MonoBehaviour
{
    public VoxelUniverse UniverseBehaviour;

    public Int3 ChunkWithin
    {
        get
        {
            var pos = VoxelUniverse.Convert(transform.position);
            var cPos = UniverseBehaviour.Universe.CalculateChunkLocalPosition(pos);
            return cPos;
        }
    }

    public bool WithinUnloadedChunk()
    {
        return !UniverseBehaviour.Universe.IsChunkLoaded(ChunkWithin);
    }
    public bool WithinUnrenderedChunk()
    {
        return !UniverseBehaviour.Universe.IsChunkRendered(ChunkWithin);
    }

    public void RequestLoad()
    {
        UniverseBehaviour.Universe.RequestChunk(ChunkWithin, true);
    }

    public bool IsLocked()
    {
        return !_controller.enabled;
    }

    private void LockPlayer()
    {
        _controller.enabled = false;
    }

    private void UnlockPlayer()
    {
        _controller.enabled = true;
    }

    private Int3 _lastChunkWithin;

    private CharacterController _controller;

    // Use this for initialization
    void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (WithinUnloadedChunk())
        {
            RequestLoad();
            LockPlayer();
        }
        else if (WithinUnrenderedChunk())
        {
            LockPlayer();
        }
        else if (IsLocked())
        {
            var delta = _lastChunkWithin - ChunkWithin;
            _lastChunkWithin = ChunkWithin;
            _controller.transform.position += (Vector3) delta;
            UnlockPlayer();
        }
        else
        {
            _lastChunkWithin = ChunkWithin;
        }
    }
}