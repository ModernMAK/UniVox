using Unity.Mathematics;
using UnityEngine;
using UniVox;
using UniVox.Types;
using UniVox.Unity;

public class PlayerInitializer : MonoBehaviour
{
    [SerializeField] private UniverseManager _universeManager;
    [SerializeField] private PlayerStreamer _streamer;
    
    [Min(4)] [SerializeField] private int _bubbleSize = 4;
    [Min(1)] [SerializeField] private int _platformSize = 1;

    private void Start()
    {
        if(_streamer == null)
            _streamer = GetComponent<PlayerStreamer>();
        if (_streamer != null)
            _streamer.enabled = false;

        for (var i = -1; i <= 1; i++)
            for (var j = -1; j <= 1; j++)
                for (var k = -1; k <= 1; k++)
                    _universeManager.ChunkManager.RequestChunkLoad(new ChunkIdentity(0, new int3(i, j, k)));
    }

    private void Update()
    {
        var chunkSize = _universeManager.ChunkManager.ChunkSize;
        var worldPosition = UnivoxUtil.ToVoxelSpace(transform.position);
        var chunkPosition = UnivoxUtil.ToChunkPosition(worldPosition, chunkSize);
        var blockPosition = UnivoxUtil.ToBlockPosition(worldPosition, chunkSize);
        var chunkId = new ChunkIdentity(0, chunkPosition);
        if(_universeManager.ChunkManager.TryGetChunkHandle(chunkId, out var handle))
        {
            handle.Handle.Complete();
            //WE could use a bresenham algorithm but I'm too lazy to bring that into this project right now.
            // Create a bubble we can move on; ignoring the lower y-hemisphere.

            //On top of that; my algo is REALLY bad at editing multiple chunks across boundaries.
            //  So we dont if it will fall out of bounds
            var flags = handle.Data.Flags;
            for (var x = -_bubbleSize; x <= _bubbleSize; x++)
                for (var y = 0; y <= _bubbleSize; y++)
                    for (var z = -_bubbleSize; z <= _bubbleSize; z++)
                    {
                        var delta = new int3(x, y, z);
                        if (x * x + y * y + z * z > _bubbleSize * _bubbleSize)
                            continue;
                        if (!UnivoxUtil.IsPositionValid(blockPosition + delta, chunkSize))
                            continue;
                        var index = UnivoxUtil.GetIndex(blockPosition + delta, chunkSize);
                        flags[index] &= ~VoxelFlag.Active;
                    }
            for (var x = -_platformSize; x <= _platformSize; x++)
                for (var z = -_platformSize; z <= _platformSize; z++)
                {
                    var delta = new int3(x, -1, z);
                    if (x * x + z * z > _platformSize * _platformSize)
                        continue;
                    if (!UnivoxUtil.IsPositionValid(blockPosition + delta, chunkSize))
                        continue;
                    var index = UnivoxUtil.GetIndex(blockPosition + delta, chunkSize);
                    flags[index] |= VoxelFlag.Active;
                }
            _universeManager.ChunkMeshManager.RequestRender(chunkId, handle);
            _universeManager.ChunkMeshManager.ProcessRenderResults(true);
            if (_streamer)
                _streamer.enabled = true;
            //Debug.Break();
            Destroy(this);
        }

    }
}
