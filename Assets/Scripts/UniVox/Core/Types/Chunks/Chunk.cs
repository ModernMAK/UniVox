using System;
using Unity.Collections;

namespace UniVox.Core
{
    public class Chunk : IDisposable, IAccessorArray<Chunk.Accessor>
    {
        public Chunk(int size = VoxelInfoArray.CubeSize, Allocator allocator = Allocator.Persistent,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            Info = new VoxelInfoArray(size, allocator, options);
            Render = new VoxelRenderInfoArray(size, allocator, options);
        }

        public VoxelInfoArray Info { get; }
        public VoxelRenderInfoArray Render { get; }


        public struct Accessor
        {
            public Accessor(Chunk chunk, int index) : this(chunk.Info, chunk.Render, index)
            {
            }

            public Accessor(VoxelInfoArray chunk, VoxelRenderInfoArray voxelRender, int index)
            {
                _coreAccessor = chunk.GetAccessor(index);
                _renderAccessor = voxelRender.GetAccessor(index);
            }

            private readonly VoxelInfoArray.Accessor _coreAccessor;
            private readonly VoxelRenderInfoArray.Accessor _renderAccessor;

            public VoxelInfoArray.Accessor Info => _coreAccessor;
            public VoxelRenderInfoArray.Accessor Render => _renderAccessor;
        }

        public void Dispose()
        {
            Info.Dispose();
            Render.Dispose();
        }

        public int Length => Info.Length;

        public Accessor this[int index] => new Accessor(this, index);

        public Accessor GetAccessor(int index)
        {
            return new Accessor(this, index);
        }
    }
}