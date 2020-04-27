using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UniVox.MeshGen;

public class UniverseChunkManager : MonoBehaviour
{
    /*
     Check if any chunks need to be loaded.
Check if any chunks have been loaded, but need to be setup (voxel configuration, set active blocks, etc...).
Check if any chunks need to be rebuilt (i.e. a chunk was modified last frame and needs mesh rebuild).
Check if any chunks need to be unloaded.
Update the chunk visibility list (this is a list of all chunks that could potentially be rendered)
Update the render list.
     */

    private Queue<int4> _chunkLoadRequests;
    private Queue<int4> _chunkGenerationRequests;
    private Queue<int4> _chunkMeshGenRequests;
    private Queue<int4> _chunkUnloadRequests;
    
    
}