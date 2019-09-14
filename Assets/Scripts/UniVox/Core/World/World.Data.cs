using System;
using Unity.Collections;

namespace UniVox.Core
{
    public partial class ChunkMap
    {
        /// <summary>
        /// Represents a Group of Data representing a Chunk
        /// </summary>
        public class Data : IDisposable
        {
            public Data(int flatSize = VoxelInfoArray.CubeSize, Allocator allocator = Allocator.Persistent,
                NativeArrayOptions options = NativeArrayOptions.ClearMemory)
            {
                CoreCore = new VoxelInfoArray(flatSize, allocator, options);
                _voxelRenderData = new VoxelRenderInfoArray(flatSize, allocator, options);
//                _chunkEntity = entity;
                _allowDispose = true;
            }

            public Data(VoxelInfoArray voxelInfoArray, VoxelRenderInfoArray voxelRenderInfoArray, bool allowDispose = true)
            {
                CoreCore = voxelInfoArray;
                _voxelRenderData = voxelRenderInfoArray;
                _allowDispose = allowDispose;
//                _chunkEntity = entity;
            }


            public readonly VoxelInfoArray CoreCore;

            private readonly VoxelRenderInfoArray _voxelRenderData;

            private readonly bool _allowDispose;
//            private readonly Entity _chunkEntity;


            public VoxelInfoArray VoxelInfoArray => CoreCore;

            public VoxelRenderInfoArray VoxelRender => _voxelRenderData;

//            public Entity Entity => _chunkEntity;
            public void Dispose()
            {
                if(!_allowDispose)
                    return;
                
                CoreCore?.Dispose();
                _voxelRenderData?.Dispose();
            }
        }
    }
}