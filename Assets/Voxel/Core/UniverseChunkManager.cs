using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Voxel.Core
{
    internal class UniverseChunkManager : IEnumerable<KeyValuePair<Int3, Chunk>>
    {
        public UniverseChunkManager(Int3 chunkSize)
        {
            ChunkSize = chunkSize;
            _loadedChunks = new Dictionary<Int3, Chunk>();
            _chunksToLoad = new Queue<Int3>();
            _chunksToUnload = new Queue<Int3>();
        }

        //FIELDS AND VARIABLES    >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        public Int3 ChunkSize { get; private set; }

        private readonly Dictionary<Int3, Chunk> _loadedChunks;
        private readonly Queue<Int3> _chunksToLoad, _chunksToUnload;


        //LOW LEVEL INTERFACE    >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        //Position Utility   >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

        /// <summary>
        /// Calculates a Local Position for the Chunk
        /// </summary>
        /// <param name="worldPosition">Position in world coordinates</param>
        /// <returns>Position in Universe's local coordinates</returns>
        public Int3 CalculateChunkLocalPosition(Int3 worldPosition)
        {
            var ratio = new Vector3(
                (float) worldPosition.x / ChunkSize.x,
                (float) worldPosition.y / ChunkSize.y,
                (float) worldPosition.z / ChunkSize.z);
            return Int3.Floor(ratio); // Int3.InvScale(worldPosition + delta, ChunkSize);
        }

        /// <summary>
        /// Calculates a world position for the Chunk.
        /// </summary>
        /// <param name="localPosition">Position in the Universe's local coordinates</param>
        /// <returns>Position in world coordinates</returns>
        public Int3 CalculateChunkWorldPosition(Int3 localPosition)
        {
            return Int3.Scale(localPosition, ChunkSize);
        }

        /// <summary>
        /// Converts any world position to a world position for the Chunk.
        /// This maps any world position within a chunk to the chunks's world position
        /// </summary>
        /// <param name="worldPosition">Position in world coordinates</param>
        /// <returns>Position in world coordinates</returns>
        public Int3 ConvertToChunkWorldPosition(Int3 worldPosition)
        {
            return CalculateChunkWorldPosition(CalculateChunkLocalPosition(worldPosition));
        }

        /// <summary>
        /// Calculates a Local Position for the Block
        /// </summary>
        /// <param name="worldPosition">Position in world coordinates</param>
        /// <returns>Position in a Chunk's local coordinates</returns>
        public Int3 CalculateBlockLocalPosition(Int3 worldPosition)
        {
            return worldPosition - ConvertToChunkWorldPosition(worldPosition);
        }

        //Get / Set Blocks    >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        private bool GetChunk(Int3 worldPosition, out Chunk chunk)
        {
            return _loadedChunks.TryGetValue(CalculateChunkLocalPosition(worldPosition), out chunk);
        }

        public bool GetBlock(Int3 worldPosition, out Block block)
        {
            Chunk chunk;
            if (GetChunk(worldPosition, out chunk))
            {
                block = chunk.GetBlock(CalculateBlockLocalPosition(worldPosition));
                return true;
            }
            block = default(Block);
            return false;
        }

        public bool SetBlock(Block block, Int3 worldPosition)
        {
            Chunk chunk;
            if (!GetChunk(worldPosition, out chunk)) return false;
            chunk.SetBlock(block, CalculateBlockLocalPosition(worldPosition));
            DirtyNeighbors(worldPosition);
            return true;
        }

        private void DirtyNeighbors(Int3 worldPosition)
        {
            var chunkPos = CalculateChunkLocalPosition(worldPosition);
            var blockPos = CalculateBlockLocalPosition(worldPosition);
            Chunk temp;
            //Sides are mutualyl exclusive
            if (blockPos.x == 0)
                if (_loadedChunks.TryGetValue(chunkPos + Int3.Left, out temp)) //Left
                    temp.Dirty();
                else
                    RequestChunkLoad(chunkPos + Int3.Left);
            else if (blockPos.x == ChunkSize.x - 1)
                if (_loadedChunks.TryGetValue(chunkPos + Int3.Right, out temp)) //Right
                    temp.Dirty();
                else
                    RequestChunkLoad(chunkPos + Int3.Right);

            if (blockPos.y == 0)
                if (_loadedChunks.TryGetValue(chunkPos + Int3.Down, out temp)) //Down
                    temp.Dirty();
                else
                    RequestChunkLoad(chunkPos + Int3.Down);
            else if (blockPos.y == ChunkSize.y - 1)
                if (_loadedChunks.TryGetValue(chunkPos + Int3.Up, out temp)) //Up
                    temp.Dirty();
                else
                    RequestChunkLoad(chunkPos + Int3.Up);

            if (blockPos.z == 0)
                if (_loadedChunks.TryGetValue(chunkPos + Int3.Back, out temp)) //Backward
                    temp.Dirty();
                else
                    RequestChunkLoad(chunkPos + Int3.Back);
            else if (blockPos.z == ChunkSize.z - 1)
                if (_loadedChunks.TryGetValue(chunkPos + Int3.Forward, out temp)) //Forward
                    temp.Dirty();
                else
                    RequestChunkLoad(chunkPos + Int3.Forward);
        }

        //HIGH LEVEL INTERFACE    >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>    
        public void Update()
        {
            UnloadingUpdate();
            LoadingUpdate();
        }

        private void LoadingUpdate()
        {
            while (_chunksToLoad.Count > 0)
            {
                var chunkLoading = _chunksToLoad.Dequeue();
                if (_loadedChunks.ContainsKey(chunkLoading))
                    continue;
                if (!LoadChunk(chunkLoading))
                    GenerateChunk(chunkLoading);
            }
        }

        private bool LoadChunk(Int3 chunkPos)
        {
            return false;
        }

        private void GenerateChunk(Int3 chunkPos)
        {
            var worldChunkPos = CalculateChunkWorldPosition(chunkPos);
            var chunk = _loadedChunks[chunkPos] = new Chunk(ChunkSize);
            foreach (var kvp in chunk)
            {
                chunk[kvp.Key] = GenerateBlock(kvp.Key + worldChunkPos);
            }
        }

        private Block GenerateBlock(Int3 worldPos)
        {
            var chunkLocalPos = CalculateChunkLocalPosition(worldPos);

            using (new SafeRandom(worldPos.GetHashCode()))
            {
                var type = (byte) Random.Range(0, 4);
                return new Block(type, (chunkLocalPos != Int3.Zero));
            }
        }

        private void UnloadingUpdate()
        {
            while (_chunksToUnload.Count > 0)
            {
                var chunkUnloading = _chunksToUnload.Dequeue();

                UnloadChunk(chunkUnloading);
            }
        }

        private bool UnloadChunk(Int3 chunk)
        {
            return _loadedChunks.Remove(chunk);
        }

        // CHUNK LOAD / UNLOAD
        public void RequestChunkLoad(Int3 localPosition)
        {
            _chunksToLoad.Enqueue(localPosition);
        }

        // CHUNK LOAD / UNLOAD
        public void RequestChunkUnload(Int3 localPosition)
        {
            _chunksToUnload.Enqueue(localPosition);
        }


        //IEnumerable    >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        public IEnumerator<KeyValuePair<Int3, Chunk>> GetEnumerator()
        {
            return _loadedChunks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsChunkLoaded(Int3 localPosition)
        {
            return _loadedChunks.ContainsKey(localPosition);
        }
    }
}