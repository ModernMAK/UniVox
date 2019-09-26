using System;
using Unity.Collections;
using UnityEdits;

namespace UniVox.Core.Types
{
    //Seriously reconsider moving to Dynamic Buffers
    //On the one hand, by having it outside the entity, its easy to manage
    //On the other, we did all this work. Refactoring before it works is extra work, refactoring after is a chore. And for what benefit?
    public class Chunk : IDisposable, IAccessorArray<Chunk.Accessor>
    {
        public Chunk(int size = UnivoxDefine.CubeSize, Allocator allocator = Allocator.Persistent,
            NativeArrayOptions options = NativeArrayOptions.ClearMemory)
        {
            Info = new VoxelInfoArray(size, allocator, options);
            Render = new VoxelRenderInfoArray(size, allocator, options);
        }

        public VoxelInfoArray Info { get; }
        public VoxelRenderInfoArray Render { get; }

        public int Length => Info.Length;

        public Accessor this[int index] => new Accessor(this, index);

        public Accessor GetAccessor(int index)
        {
            return new Accessor(this, index);
        }

        public void Dispose()
        {
            Info.Dispose();
            Render.Dispose();
        }


        public struct Accessor
        {
            public Accessor(Chunk chunk, int index) : this(chunk.Info, chunk.Render, index)
            {
            }

            public Accessor(VoxelInfoArray chunk, VoxelRenderInfoArray voxelRender, int index)
            {
                Info = chunk.GetAccessor(index);
                Render = voxelRender.GetAccessor(index);
            }

            public VoxelInfoArray.Accessor Info { get; }

            public VoxelRenderInfoArray.Accessor Render { get; }
        }
    }
}