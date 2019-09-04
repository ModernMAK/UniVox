using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Types
{
    public class ChunkManagerV2 : IDisposable
    {
        private class ChunkMetadata : IDisposable
        {
            public Chunk Chunk;
            public bool Valid;

            public void Dispose()
            {
                Chunk?.Dispose();
            }
        }


        private readonly DisposableDictionary<int3, ChunkMetadata> Chunks;
        private readonly DisposableDelegatePool<Chunk> _chunkDelegatePool;


        public ChunkManagerV2()
        {
            Chunks = new DisposableDictionary<int3, ChunkMetadata>();
            _chunkDelegatePool = new DisposableDelegatePool<Chunk>(() => new Chunk());
        }

        public IEnumerable<int3> Loaded => Chunks.Keys;

        public int LoadedCount => Chunks.Count;


        public void Load(int3 position)
        {
            if (!Chunks.ContainsKey(position))
            {
                var chunk = _chunkDelegatePool.Acquire();
                Chunks[position] = new ChunkMetadata()
                {
                    Chunk = chunk,
                    Valid = false
                };
            }
        }


        public bool IsValid(int3 position)
        {
            if (Chunks.TryGetValue(position, out var value))
            {
                return value.Valid;
            }

            return false;
        }

        public bool TryGetChunk(int3 position, out Chunk chunk)
        {
            if (Chunks.TryGetValue(position, out var value))
            {
                chunk = value.Chunk;
                return true;
            }

            chunk = default;
            return false;
        }

        public bool MarkValid(int3 position, bool valid = true)
        {
            if (Chunks.TryGetValue(position, out var value))
            {
                value.Valid = valid;
                return true;
            }

            return false;
        }

        public void Unload(int3 position)
        {
            if (Chunks.TryGetValue(position, out var value))
            {
                _chunkDelegatePool.Release(value.Chunk);
                Chunks.Remove(position);
            }
        }


        public void Dispose()
        {
            Chunks?.Dispose();
            _chunkDelegatePool?.Dispose();
        }
    }
}