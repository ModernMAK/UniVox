//using System;
//using System.Collections.Generic;
//using ProceduralMesh;
//using UnityEngine;
//
//namespace Voxel.Core
//{
//
//    public class RendererUtil
//    {
//        public static readonly VoxelRenderMode[] Modes =
//        {
//            VoxelRenderMode.Trigger, VoxelRenderMode.Solid
//        };
//
//
//        private DynamicMesh Mesh { get; set; }
//        private Dictionary<Int3, Mesh[][]> ChunkCache { get; set; }
//        private Mesh[][] TotalCache { get; set; }
//        private bool IsCacheDirty;
//
//        public RendererUtil()
//        {
//            Mesh = new DynamicMesh();
//            ChunkCache = new Dictionary<Int3, Mesh[][]>();
//            TotalCache = new Mesh[2][] {new Mesh[0], new Mesh[0]};
//            IsCacheDirty = false;
//        }
//
//
//        public Mesh[] GetValue(VoxelRenderMode mode, Int3 pos)
//        {
//            return ChunkCache[pos][(int) mode];
//        }
//
//        public bool TryGetValue(VoxelRenderMode mode, Int3 pos, out Mesh[] meshes)
//        {
//            Mesh[][] meshArr;
//            if (TryGetValue(pos, out meshArr))
//            {
//                meshes = meshArr[(int) mode];
//                return true;
//            }
//            else
//            {
//                meshes = default(Mesh[]);
//                return false;
//            }
//        }
//
//        private bool TryGetValue(Int3 pos, out Mesh[][] meshes)
//        {
//            return ChunkCache.TryGetValue(pos, out meshes);
//        }
//
//        public bool Remove(Int3 pos)
//        {
//            return ChunkCache.Remove(pos);
//        }
//
//        private void UpdateCache(IEnumerable<Chunk> chunks)
//        {
//            foreach (var mode in Modes)
//            {
//                TotalCache[(int) mode] = UpdateColliders(mode, chunks);
//            }
//            IsCacheDirty = false;
//        }
//
//        public Mesh[] RenderFromCache(VoxelRenderMode mode, IEnumerable<Chunk> chunks)
//        {
//            if (IsCacheDirty)
//            {
//                UpdateCache(chunks);
//            }
//            return TotalCache[(int) mode];
//        }
//
//        public Mesh[] UpdateColliders(VoxelRenderMode mode, IEnumerable<Chunk> chunks, bool forceRender = false)
//        {
//            var meshes = new List<Mesh>();
//            foreach (var chunk in chunks)
//            {
//                meshes.AddRange(UpdateColliders(mode, chunk, forceRender));
//            }
//            return meshes.ToArray();
//        }
//
//        public Mesh[] UpdateColliders(VoxelRenderMode mode, Chunk chunk, bool forceRender = false)
//        {
//            Mesh[] meshes;
//            if (forceRender || !TryGetValue(mode, chunk.ChunkPosition, out meshes))
//                meshes = ForceRender(chunk)[(int) mode];
//
//            return meshes;
//        }
//
//        private Mesh[] ForceRenderTransparent(Chunk chunk)
//        {
//            var chunkMeshes = new List<Mesh>();
//            Mesh.Clear();
//            HashSet<Int3> processed = new HashSet<Int3>();
//            Queue<Int3> positions = new Queue<Int3>(chunk.Positions());
//            //A log (like a literal log) approach to optimizatins, 
//            while (positions.Count > 0)
//            {
//                var pos = positions.Dequeue();
//                if (processed.Contains(pos))
//                    continue;
//                var block = chunk[pos];
//
//                if (!block.Active || !block.IsTransparent())
//                {
//                    processed.Add(pos);
//                    continue;
//                }
//                VoxelDirection logDir = VoxelDirection.Up;
//                block.RenderPass(Mesh);
//                processed.Add(pos);
//                var tempPos = pos;
//                do
//                {
//                    tempPos += logDir.ToVector();
//
//                    if (processed.Contains(tempPos))
//                        break;
//                    if (!chunk.IsValid(tempPos))
//                        break;
//
//                    var tempBlock = chunk[tempPos];
//
//                    if (tempBlock.Active && tempBlock.IsTransparent())
//                    {
//                        tempBlock.RenderPass(Mesh);
//                        processed.Add(tempPos);
//                    }
//                    else break;
//                } while (true);
//                tempPos = pos;
//                do
//                {
//                    tempPos -= logDir.ToVector();
//
//                    if (processed.Contains(tempPos))
//                        break;
//                    if (!chunk.IsValid(tempPos))
//                        break;
//
//                    var tempBlock = chunk[tempPos];
//
//                    if (tempBlock.Active && tempBlock.IsTransparent())
//                    {
//                        tempBlock.RenderPass(Mesh);
//                        processed.Add(tempPos);
//                    }
//                    else break;
//                } while (true);
//                chunkMeshes.AddRange(Mesh.Compile());
//                Mesh.Clear();
//            }
//            return chunkMeshes.ToArray();
//        }
//
//        private Mesh[] ForceRenderSolid(Chunk chunk)
//        {
//            Mesh.Clear();
//
//            foreach (var pos in chunk.Positions())
//            {
//                var block = chunk[pos];
//                if (!block.Active)
//                    continue;
//
//                if (!block.IsTransparent())
//                    block.RenderPass(Mesh);
//                break;
//            }
//
//            return Mesh.Compile();
//        }
//
//        public Mesh[][] ForceRender(Chunk chunk)
//        {
//            Mesh[][] cacheData = new Mesh[2][];
//            IsCacheDirty = true;
//            foreach (var mode in Modes)
//            {
//                Mesh[] temp = null;
//                switch (mode)
//                {
//                    case VoxelRenderMode.Trigger:
//                        temp = ForceRenderTransparent(chunk);
//                        break;
//                    case VoxelRenderMode.Solid:
//                        temp = ForceRenderSolid(chunk);
//                        break;
//                    default:
//                        throw new ArgumentException();
//                }
//                for (var i = 0; i < temp.Length; i++)
//                {
//                    var mName = string.Format("Chunk {0}", chunk.ChunkPosition.ToString());
//                    if (temp.Length > 1)
//                        mName += string.Format(" {0}/{1}", (i + 1), temp.Length);
//                    temp[i].name = mName;
//                }
//                cacheData[(int) mode] = temp;
//            }
//            ChunkCache[chunk.ChunkPosition] = cacheData;
//            return cacheData;
//        }
//    }
//}