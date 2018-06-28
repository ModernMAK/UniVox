using UnityEngine;
using Voxel.Blocks;

namespace Voxel.Core
{
    public class Universe
    {
        public Universe(Int3 chunkSize)
        {
            _chunkManager = new UniverseChunkManager(chunkSize);
            _internalRendererCache = new RendererCache();
            _internalColliderCache = new ColliderCache();
            
//            _blockManager = new BlockManager();

//            _blockManager.AddReference(new GrassBlock());
//            _blockManager.AddReference(new DirtBlock());
//            _blockManager.AddReference(new StoneBlock());
//            _blockManager.AddReference(new SandBlock());
//
//            _blockManager.AddReference(new CoalBlock());
//            _blockManager.AddReference(new CopperBlock());
//            
//            _blockManager.AddReference(new LiquidBlock());
        }

        //FIELDS AND VARIABLES    >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        private readonly RendererCache _internalRendererCache;
        private readonly ColliderCache _internalColliderCache;

        private readonly UniverseChunkManager _chunkManager;
//        private readonly BlockManager _blockManager;


//        public BlockManager BlockManager
//        {
//            get { return _blockManager; }
//        }

        public void Update()
        {
            //Unload and LoadChunks
            _chunkManager.Update();

            Tick();
            SimulationTick();

            Render();
        }

        private void Tick()
        {
            foreach (var chunkPair in _chunkManager)
            {
                var chunk = chunkPair.Value;
                foreach (var blockPair in chunk)
                {
                    chunk[blockPair.Key] = VoxelManager.Blocks.Tick(blockPair.Value);
                }
            }
        }

        private void SimulationTick()
        {
//            foreach (var chunkPair in _chunkManager)
//            {
//                var chunk = chunkPair.Value;
//                var chunkWorldPos = CalculateChunkWorldPosition(chunkPair.Key);
//                foreach (var blockPair in chunk)
//                {
//                    var block = blockPair.Value;
//                    var blockWorldPos = blockPair.Key + chunkWorldPos;
//                    var newBlock = chunk[blockPair.Key] =
//                        _blockManager.SimulationTick(block, blockWorldPos, this);
//                    if (!block.Equals(newBlock))
//                        chunk.Dirty();
//                }
//            }
        }

        //BlockManager Utility Funcs 

        /// <summary>
        /// Calculates a Local Position for the Chunk
        /// </summary>
        /// <param name="worldPosition">Position in world coordinates</param>
        /// <returns>Position in Universe's local coordinates</returns>
        public Int3 CalculateChunkLocalPosition(Int3 worldPosition)
        {
            return _chunkManager.CalculateChunkLocalPosition(worldPosition);
        }

        /// <summary>
        /// Calculates a world position for the Chunk.
        /// </summary>
        /// <param name="localPosition">Position in the Universe's local coordinates</param>
        /// <returns>Position in world coordinates</returns>
        public Int3 CalculateChunkWorldPosition(Int3 localPosition)
        {
            return _chunkManager.CalculateChunkWorldPosition(localPosition);
        }

        /// <summary>
        /// Converts any world position to a world position for the Chunk.
        /// This maps any world position within a chunk to the chunks's world position
        /// </summary>
        /// <param name="worldPosition">Position in world coordinates</param>
        /// <returns>Position in world coordinates</returns>
        public Int3 ConvertToChunkWorldPosition(Int3 worldPosition)
        {
            return _chunkManager.ConvertToChunkWorldPosition(worldPosition);
        }

        /// <summary>
        /// Calculates a Local Position for the Block
        /// </summary>
        /// <param name="worldPosition">Position in world coordinates</param>
        /// <returns>Position in a Chunk's local coordinates</returns>
        public Int3 CalculateBlockLocalPosition(Int3 worldPosition)
        {
            return _chunkManager.CalculateBlockLocalPosition(worldPosition);
        }


        //Rendering    >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>


        private void Render()
        {
            _internalRendererCache.Render(_chunkManager);
            _internalColliderCache.UpdateColliders(_chunkManager);
            foreach (var chunk in _chunkManager)
            {
                chunk.Value.Clean();                
            }
        }

        public Mesh[] GetMeshes(VoxelRenderMode mode)
        {
            return _internalRendererCache.RetrieveCache(mode, _chunkManager);
        }
        public Mesh[] GetColliders(VoxelCollisionMode mode)
        {
            return _internalColliderCache.RetrieveCache(mode, _chunkManager);
        }


        public void RequestChunk(Int3 chunkPosition, bool requestLoad)
        {
            if (requestLoad)
                _chunkManager.RequestChunkLoad(chunkPosition);
            else
                _chunkManager.RequestChunkUnload(chunkPosition);
        }


        //GET / SET BLOCK   >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        public bool GetBlock(Int3 position, out Block block)
        {
            return _chunkManager.GetBlock(position, out block);
        }

        public bool SetBlock(Block block, Int3 position)
        {
            return _chunkManager.SetBlock(block, position);
        }

        public bool IsChunkLoaded(Int3 position, bool isLocal = true)
        {
            if (!isLocal)
                position = CalculateChunkLocalPosition(position);
            return _chunkManager.IsChunkLoaded(position);
        }

        public bool IsChunkRendered(Int3 position, bool isLocal = true)
        {
            if (!isLocal)
                position = CalculateChunkLocalPosition(position);
            return _internalRendererCache.IsChunkRendered(position);
        }

        public void DropItem(Int3 blockWorldPos, Item item)
        {
            
        }
    }
}
    