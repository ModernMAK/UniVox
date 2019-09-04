using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Types
{
    public class ChunkTableManager : IDisposable
    {
        private readonly IDictionary<int3, Chunk> _chunkLookup;

        public ChunkTableManager()
        {
            _chunkLookup = new Dictionary<int3, Chunk>();
        }

        public IEnumerable<int3> Loaded => _chunkLookup.Keys;
        public int LoadedCount => _chunkLookup.Count;

        public void Dispose()
        {
            foreach (var key in _chunkLookup.Keys) DisposeAt(key);
        }

        public void UnsafeLoad(int3 position, Chunk chunk)
        {
            _chunkLookup[position] = chunk;
        }

        public void TransferTo(int3 position, ChunkTableManager ctm)
        {
            if (_chunkLookup.TryGetValue(position, out var chunk))
            {
                ctm.Load(position, chunk);
                UnsafeUnload(position);
            }
        }

        public void UnsafeTransferTo(int3 position, ChunkTableManager ctm)
        {
            var chunk = Get(position);
            ctm.UnsafeLoad(position, chunk);
            UnsafeUnload(position);
        }

        public bool UnsafeUnload(int3 position)
        {
            return _chunkLookup.Remove(position);
        }

        public void Load(int3 position, Chunk chunk)
        {
            if (IsLoaded(position))
                Unload(position);
            UnsafeLoad(position, chunk);
        }

        public void Unload(int3 position)
        {
            //TODO this check is neccessary, find out why
            if (_chunkLookup.ContainsKey(position))
            {
                DisposeAt(position);
                UnsafeUnload(position);
            }
        }

        public Chunk Get(int3 position)
        {
            return _chunkLookup[position];
        }

        public bool IsLoaded(int3 position)
        {
            return _chunkLookup.ContainsKey(position);
        }

        private void DisposeAt(int3 key)
        {
            _chunkLookup[key].Dispose();
        }
    }
}