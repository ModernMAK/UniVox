using System;
using Unity.Collections;
using Unity.Entities;

namespace Univox
{
    public partial class World
    {
        /// <summary>
        /// Represents a Group of Data representing a Chunk
        /// </summary>
        public class Record : IDisposable
        {
            public Record(Entity entity, int flatSize = Chunk.CubeSize, Allocator allocator = Allocator.Persistent,
                NativeArrayOptions options = NativeArrayOptions.ClearMemory)
            {
                _coreData = new Chunk(flatSize, allocator, options);
                _renderData = new RenderChunk(flatSize, allocator, options);
                _chunkEntity = entity;
            }


            public readonly Chunk _coreData;
            private readonly RenderChunk _renderData;
            private readonly Entity _chunkEntity;


            public Chunk Chunk => _coreData;
            public RenderChunk Render => _renderData;
            public Entity Entity => _chunkEntity;

            public void Dispose()
            {
                _coreData?.Dispose();
                _renderData?.Dispose();
            }
        }
    }
}