using Unity.Jobs;
using UnityEngine;
using UniVox.Types;
using UniVox.Unity;

//
//public class UniverseInterface : MonoBehaviour
//{
//    public void Initialize(UniverseChunkManager manager) => ChunkManager = manager;
//    public UniverseChunkManager ChunkManager { get; private set; }
//
//    
////    
////    public bool TryGetChunk(VoxelIdentity voxelIdentity, out PersistentDataHandle<VoxelChunk> block)
////    {
////        if (ChunkManager.TryGetChunk(voxelIdentity.CreateChunkId(), out var chunk))
////        {
////            block = chunk.GetBlock(voxelIdentity.VoxelId);
////            return true;
////        }
////
////        block = default;
////        return false;
////    }
////    
////    public bool TryGetBlock(VoxelIdentity voxelIdentity, out VoxelBlock block)
////    {
////        if (ChunkManager.TryGetChunk(voxelIdentity.CreateChunkId(), out var chunk))
////        {
////            block = chunk.GetBlock(voxelIdentity.VoxelId);
////            return true;
////        }
////
////        block = default;
////        return false;
////    }
////
////    public bool TrySetBlock(VoxelIdentity voxelIdentity, VoxelBlock block)
////    {
////        if (ChunkManager.TryGetChunk(voxelIdentity.CreateChunkId(), out var chunk))
////        {
////            chunk.SetBlock(voxelIdentity.VoxelId, block);
////            return true;
////        }
////
////
////        return false;
////    }
//


//}
public class PersistentDataHandle<T>
{
    public PersistentDataHandle(T data, JobHandle handle)
    {
        Data = data;
        Handle = handle;
    }

    public T Data { get; }
    public JobHandle Handle { get; private set; }

    public JobHandle DependOn(JobHandle handle)
    {
        Handle = JobHandle.CombineDependencies(Handle, handle);
        return handle;
    }
}