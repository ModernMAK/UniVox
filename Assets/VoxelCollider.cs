using System;
using System.Collections.Generic;
using ProceduralMesh;
using UnityEngine;
using Voxel.Core;
using Universe = Voxel.Core.Universe;

namespace Voxel.Unity
{
    [RequireComponent(typeof(VoxelUniverse))]
    public class VoxelCollider : MonoBehaviour
    {
//        public bool ForceRender = false;
        [SerializeField] [EnumFlags] private VoxelCollisionModeFlag _modeFlag;
        private VoxelUniverse _behaviour;
        [SerializeField] 

        [Serializable]
        public struct VoxelRenderCacheData
        {
            public VoxelCollisionMode Mode; // { get; private set; }
            public DynamicMeshCollider SubCollider { get; private set; }
            //public List<Mesh> ChunkMeshes { get; private set; }

            public VoxelCollisionModeFlag Flag
            {
                get { return (VoxelCollisionModeFlag) (1 << (int) Mode); }
            }


            public void Initialize(GameObject g, VoxelCollisionMode mode)
            {
                Mode = mode;
            //    ChunkMeshes = new List<Mesh>();
                SubCollider = CreateSubCollider(g);
//                Collider = CreateCollider(g);
            }

            public void Initialize(Component c, VoxelCollisionMode mode)
            {
                Initialize(c.gameObject, mode);
            }

            private DynamicMeshCollider CreateSubCollider(GameObject go)
            {
                var dmr = go.AddComponent<DynamicMeshCollider>();
                dmr.ContainerName = Mode.ToString();
                return dmr;
            }
        }
        private DynamicMeshCollider CreateCollider(GameObject go)
        {
            var dmc = go.AddComponent<DynamicMeshCollider>();
            dmc.ContainerName = "Colliders";
            return dmc;
        }


        public VoxelRenderCacheData Solid;
        public VoxelRenderCacheData Trigger;

//
//        private IEnumerable<Chunk> RenderQueue
//        {
//            get { return Universe.GetLoadedChunks(); }
//        }

        public Universe Universe
        {
            get { return _behaviour.Universe; }
        }

        protected virtual void Awake()
        {
            _behaviour = GetComponent<VoxelUniverse>();
            Solid.Initialize(this, VoxelCollisionMode.Solid);
            Trigger.Initialize(this, VoxelCollisionMode.Trigger);
        }

//        protected virtual void Start()
//        {
////            ForceRender = true;
//        }

//
//
        protected virtual void Update()
        {
//            if (ForceRender) // || Universe.IsDirty)
//            {
//                Debug.Log("R");
                UpdateColliders();
////                Universe.Clean();
////                shouldClean = true;
//                ForceRender = false;
//            }
        }

//        protected void ValidateCache()
//        {
//            var temp = Universe.LoadedChunks.Select(x => x.ChunkPosition).ToArray();
//            Queue<Int3> invalid = new Queue<Int3>();
//            foreach (var cacheChunk in ColliderUtil.Keys)
//            {
//                if (!temp.Contains(cacheChunk))
//                    invalid.Enqueue(cacheChunk);
//            }
//            while (invalid.Count > 0)
//            {
//                ColliderUtil.Remove(invalid.Dequeue());
//            }
//        }
//
        public IEnumerable<VoxelRenderCacheData> ActiveHelpers
        {
            get
            {
                //Solid & Trigger are type helpers
                if (_modeFlag.HasFlag(Solid.Flag))
                    yield return Solid;
                if (_modeFlag.HasFlag(Trigger.Flag))
                    yield return Trigger;
            }
        }

        protected void UpdateColliders()
        {
            foreach (var helper in ActiveHelpers)
            {
                helper.SubCollider.SetMeshes(Universe.GetColliders(helper.Mode));
            }
//            foreach (var chunk in Universe)
//            {
//                foreach (var helper in ActiveHelpers)
//                {
//                    Mesh[] m;
//                    if (!helper.Cache.TryGetValue(chunk.ChunkPosition, out m))
//                    {
//                        m = RenderChunk(chunk, helper);
//                    }
//                    for (var i = 0; i < m.Length; i++)
//                    {
//                        var mName = string.Format("Chunk {0}", chunk.ChunkPosition.ToString());
//                        if (m.Length > 1)
//                            mName += string.Format(" {0}/{1}", (i + 1), m.Length);
//                        m[i].name = mName;
//                    }
//                    helper.ChunkMeshes.AddRange(m);
//                }
////                chunk.Clean();
//            }
//            foreach (var helper in ActiveHelpers)
//            {
//            }
        }
//
//        protected Mesh[] RenderChunk(Chunk chunk, VoxelRenderCacheData helper)
//        {
//            var chunkMeshes = new List<Mesh>();
//            var mesh = helper.Mesh;
//            mesh.Clear();
//
//            foreach (var pos in Int3.RangeEnumerable(chunk.Size))
//            {
//                var block = chunk.GetBlock(pos);
//                switch (helper.Mode)
//                {
//                    case VoxelCollisionMode.Trigger:
//                        if (block.IsTransparent())
//                            block.RenderPass(pos, chunk, mesh);
//                        chunkMeshes.AddRange(OptomizeMeshes(mesh.Compile()));
//                        mesh.Clear();
//                        break;
//                    case VoxelCollisionMode.Solid:
//                        if (!block.IsTransparent())
//                            block.RenderPass(pos, chunk, mesh);
//                        break;
//                }
//            }
//            chunkMeshes.AddRange(OptomizeMeshes(mesh.Compile()));
//            return helper.Cache[chunk.ChunkPosition] = chunkMeshes.ToArray();
//        }
//
//        private Mesh[] OptomizeMeshes(Mesh[] m)
//        {
//            if (!ShouldOptomizeMeshes) return m;
//            foreach (var t in m)
//                MeshUtility.Optimize(t);
//            return m;
//        }

//
//        protected Mesh[] RenderUniverse()
//        {
//            List<Mesh> meshes = new List<Mesh>();
//            foreach (var chunk in Universe.LoadedChunks)
//            {
//                Mesh[][] m = new Mesh[2][];
//                if (chunk.IsDirty || !ColliderUtil.TryGetValue(chunk.ChunkPosition, out m))
//                {
//                    m[0] = RenderChunk(chunk);
//                    for (var i = 0; i < m.Length; i++)
//                    {
//                        var mName = string.Format("Chunk {0}", chunk.ChunkPosition.ToString());
//                        if (m.Length > 1)
//                            mName += string.Format(" {0}/{1}", (i + 1), m.Length);
//                        m[0][i].name = mName;
//                    }
//                    ColliderUtil[chunk.ChunkPosition] = m;
////                    chunk.Clean();
//                }
//                meshes.AddRange(m);
//            }
//            _dm.Clear();
//            return meshes.ToArray();
//        }
////
////            private Mesh[] RenderChunkTransparent(Chunk chunk)
////            {
////                var chunkMeshes = new List<Mesh>();
////                _dm.Clear();
////                foreach (var block in chunk)
////                    if (block != null)
////                    {
////                        _dm.Clear();
////                        block.TransparentRenderPass(_dm);
////                        chunkMeshes.AddRange(_dm.Compile());
////                    }
////
////                return chunkMeshes.ToArray();
////            }
////
////            private Mesh[] RenderChunkOpaque(Chunk chunk)
////            {
////                _dm.Clear();
////                foreach (var block in chunk)
////                    if (block != null)
////                        block.OpaqueRenderPass(_dm);
////                return _dm.Compile();
////            }
//
    }
}