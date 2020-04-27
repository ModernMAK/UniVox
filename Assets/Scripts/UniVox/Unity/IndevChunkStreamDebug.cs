using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Types;

[RequireComponent(typeof(UniverseManager))]
public class IndevChunkStreamDebug : MonoBehaviour
{
    public int WorldRequested;

    public bool UseRequested;
    public int3 ChunkRequested;
    public ChunkIdentity ChunkIdentityRequested => new ChunkIdentity(WorldRequested, ChunkRequested);

    public bool UseRange;
    public int RangeWorld;
    public int3 Minimum;
    public int3 Maximum;



    public bool LoadFlag;
    public bool UnloadFlag;

    private UniverseManager _universeManager;

    private void Awake()
    {
        _universeManager = GetComponent<UniverseManager>();
    }

    private IEnumerable<ChunkIdentity> GenerateRange()
    {
        var minX = Mathf.Min(Minimum.x, Maximum.x);
        var maxX = Mathf.Max(Minimum.x, Maximum.x);
        var minY = Mathf.Min(Minimum.y, Maximum.y);
        var maxY = Mathf.Max(Minimum.y, Maximum.y);
        var minZ = Mathf.Min(Minimum.z, Maximum.z);
        var maxZ = Mathf.Max(Minimum.z, Maximum.z);


        for (var x = minX; x <= maxX; x++)
        for (var y = minY; y <= maxY; y++)
        for (var z = minZ; z <= maxZ; z++)
            yield return new ChunkIdentity(RangeWorld, new int3(x, y, z));
    }

    private void Update()
    {
        if (LoadFlag)
        {
            if (UseRequested)
                _universeManager.ChunkManager.RequestChunkLoad(ChunkIdentityRequested);
            if (UseRange)
            {
                foreach (var chunkId in GenerateRange())
                {
                    _universeManager.ChunkManager.RequestChunkLoad(chunkId);
                }
            }

            LoadFlag = false;
        }

        if (UnloadFlag)
        {
            if (UseRequested)
                _universeManager.ChunkManager.RequestChunkUnload(ChunkIdentityRequested);
            if (UseRange)
            {
                foreach (var chunkId in GenerateRange())
                {
                    _universeManager.ChunkManager.RequestChunkUnload(chunkId);
                }
            }

            UnloadFlag = false;
        }
    }
}