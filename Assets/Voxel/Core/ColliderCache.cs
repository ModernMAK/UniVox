using System.Collections.Generic;
using ProceduralMesh;
using UnityEngine;

namespace Voxel.Core
{
    internal class ColliderCache
    {
        private readonly Dictionary<Int3, Mesh[][]> _meshCache;
        private readonly DynamicMesh _solidMesh;
        private readonly DynamicMesh _triggerMesh;
        private const int MeshTypes = 2;
        private readonly Mesh[][] _meshCollectionCache;
        private bool _meshCollectionDirty;

        public ColliderCache()
        {
            _meshCache = new Dictionary<Int3, Mesh[][]>();
            _solidMesh = new DynamicMesh();
            _triggerMesh = new DynamicMesh();
            _meshCollectionCache = new Mesh[MeshTypes][];
            _meshCollectionDirty = true;
        }

        public Mesh[][] RetrieveCache(IEnumerable<KeyValuePair<Int3, Chunk>> chunks)
        {
            if (_meshCollectionDirty)
                CollectMeshes(chunks);
            return _meshCollectionCache;
        }

        public Mesh[] RetrieveCache(VoxelCollisionMode mode, IEnumerable<KeyValuePair<Int3, Chunk>> chunks)
        {
            return RetrieveCache(chunks)[(int) mode];
        }

        private void CollectMeshes(IEnumerable<KeyValuePair<Int3, Chunk>> chunks)
        {
            var renderBuilder = new List<Mesh>[MeshTypes];
            for (var i = 0; i < MeshTypes; i++)
                renderBuilder[i] = new List<Mesh>();

            foreach (var chunkKvp in chunks)
            {
                Mesh[][] data;
                if (!_meshCache.TryGetValue(chunkKvp.Key, out data)) continue;
                for (var i = 0; i < MeshTypes; i++)
                    renderBuilder[i].AddRange(data[i]);
            }
            for (var i = 0; i < MeshTypes; i++)
                _meshCollectionCache[i] = renderBuilder[i].ToArray();
            _meshCollectionDirty = false;
        }


        public void UpdateColliders(IEnumerable<KeyValuePair<Int3, Chunk>> chunks)
        {
            UpdateColliders(chunks,VoxelManager.Blocks);
        }

        public void UpdateColliders(IEnumerable<KeyValuePair<Int3, Chunk>> chunks, BlockManager manager)
        {
            foreach (var chunkKvp in chunks)
            {
                var chunkPos = chunkKvp.Key;
                var chunk = chunkKvp.Value;
                if (_meshCache.ContainsKey(chunkPos) && !chunk.BlocksUpdated) continue;
                UpdateCache(chunkPos, chunk, manager);
//                chunk.Clean();
                _meshCollectionDirty = true;
            }
        }

        private Mesh[][] CollisionChunk(Int3 chunkPos, Chunk chunk, BlockManager manager)
        {
            _solidMesh.Clear();
            _triggerMesh.Clear();
            var chunkWorldPos = Int3.Scale(chunkPos, chunk.Size);
            foreach (var blockPair in chunk)
            {
                var blockLocalPos = blockPair.Key;
                var block = blockPair.Value;
                var blockWorldPos = blockLocalPos + chunkWorldPos;

                foreach (var dir in VoxelDirectionExt.Directions)
                {
                    if (manager.ShouldRenderCollider(block, blockLocalPos, chunk, dir))
                    {
                        manager.RenderCollider(block, blockWorldPos, dir,
                            manager.CollisionType(block) == BlockCollisionType.Solid ? _solidMesh : _triggerMesh);
                    }
                }
            }
            var data = new[] { _solidMesh.Compile(), _triggerMesh.Compile()};
            return RenameMeshes(chunkPos, chunk, data);
        }

        private static Mesh[][] RenameMeshes(Int3 chunkPos, Chunk chunk, Mesh[][] data)
        {
            for (var mode = 0; mode < MeshTypes; mode++)
            {
                var modeData = data[mode];
                for (var meshIndex = 0; meshIndex < modeData.Length; meshIndex++)
                {
                    var name = string.Format("Chunk {0}", chunkPos);
                    if (modeData.Length > 1)
                        name += string.Format(" {0} / {1}", meshIndex + 1, modeData.Length);

                    modeData[meshIndex].name = name;
                }
            }
            return data;
        }

        private Mesh[][] FetchCache(Int3 chunkPos, Chunk chunk, BlockManager manager)
        {
            Mesh[][] data;
            if (!_meshCache.TryGetValue(chunkPos, out data))
                data = UpdateCache(chunkPos, chunk, manager);
            return data;
        }

        private Mesh[][] UpdateCache(Int3 chunkPos, Chunk chunk, BlockManager manager)
        {
            return _meshCache[chunkPos] = CollisionChunk(chunkPos, chunk, manager);
//            return UpdateCache(chunk.ChunkPosition, CollisionChunk(chunk));
        }
//
//        private Mesh[][] UpdateCache(Int3 chunkPos, Mesh[][] data)
//        {
//            return _meshCache[chunkPos] = data;
//        }

        //Remove a chunk from the cache, will force a re
        public bool RemoveFromCache(Int3 chunkPos)
        {
            return _meshCache.Remove(chunkPos);
//            return RemoveFromCache(chunk.ChunkPosition);
        }

//        private bool RemoveFromCache(Int3 chunkPos)
//        {
//            return _meshCache.Remove(chunkPos);
//        }
        public bool IsChunkCollisioned(Int3 localPosition)
        {
            return _meshCache.ContainsKey(localPosition);
        }
    }
}