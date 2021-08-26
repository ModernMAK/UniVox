using System;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UniVox.MeshGen;
using UniVox.Types;
using UniVox.Utility;

namespace UniVox.Unity
{
    [RequireComponent(typeof(UniverseChunkIO))]
    [RequireComponent(typeof(UniverseChunkGenerator))]
    public class UniverseChunkManager : MonoBehaviour
    {
        /*
     Check if any chunks need to be loaded.
Check if any chunks have been loaded, but need to be setup (voxel configuration, set active blocks, etc...).
Check if any chunks need to be rebuilt (i.e. a chunk was modified last frame and needs mesh rebuild).
Check if any chunks need to be unloaded.
Update the chunk visibility list (this is a list of all chunks that could potentially be rendered)
Update the render list.
     */

        private enum LoadRequestType
        {
            Loading,
            Unloading
        }

        private struct Request
        {
            public ChunkIdentity ChunkId { get; set; }
            public LoadRequestType Type { get; set; }
        }

        [SerializeField] private int _requestsPerFrame = 1;

        [SerializeField] private int _maxRequestsGenerating = 8; //Pretty okay number, enough to let some generate, but not enough to cause any major stalling

        [SerializeField] private int3 _chunkSize;

        private Queue<Request> _chunkLoadRequests;

        private Dictionary<ChunkIdentity, PersistentDataHandle<VoxelChunk>> _chunksLoaded;
        private LinkedList<DataHandle<Tuple<ChunkIdentity, VoxelChunk>>> _chunksGenerating;

        private UniverseChunkIO _universeChunkIo;
        private UniverseChunkGenerator _universeChunkGenerator;
        private UniverseManager _universeManager;
        public void InitializeManager(UniverseManager manager) => _universeManager = manager;


        private void Awake()
        {
            _universeChunkIo = GetComponent<UniverseChunkIO>();
            _chunksGenerating = new LinkedList<DataHandle<Tuple<ChunkIdentity, VoxelChunk>>>();
            _chunkLoadRequests = new Queue<Request>();
            _chunksLoaded = new Dictionary<ChunkIdentity, PersistentDataHandle<VoxelChunk>>();
            _universeChunkGenerator = GetComponent<UniverseChunkGenerator>();
//        _voxelChunkGenerator = new VoxelChunkGenerator(){}
        }
        private void OnDestroy()
        {
            _chunkLoadRequests.Clear();

            foreach (var chunkPair in _chunksLoaded)
            {
                var persistantDataHandle = chunkPair.Value;
                persistantDataHandle.Handle.Complete();
                persistantDataHandle.Data.Dispose();
            }
            _chunksLoaded.Clear();

            foreach (var dataHandle in _chunksGenerating)
            {
                dataHandle.Handle.Complete();
                dataHandle.Data.Item2.Dispose();
            }
            _chunksGenerating.Clear();


        }

        private void Update()
        {
            ProcessRequests(_requestsPerFrame);
        }


        public void RequestChunkLoad(ChunkIdentity chunkId)
        {
            var request = new Request()
            {
                ChunkId = chunkId,
                Type = LoadRequestType.Loading
            };
            _chunkLoadRequests.Enqueue(request);
        }

        public void RequestChunkUnload(ChunkIdentity chunkId)
        {
            var request = new Request()
            {
                ChunkId = chunkId,
                Type = LoadRequestType.Unloading
            };
            _chunkLoadRequests.Enqueue(request);
        }

        private void CheckGeneratingQueue()
        {
            var current = _chunksGenerating.First;
            while (current != null)
            {
                var next = current.Next;
                var handle = current.Value.Handle;
                var chunkId = current.Value.Data.Item1;
                var chunk = current.Value.Data.Item2;

                if (handle.IsCompleted)
                {
                    handle.Complete(); //We have to do this because of unity's job  safety system
                    _chunksGenerating.Remove(current);
                    var chunkHadle = _chunksLoaded[chunkId] = new PersistentDataHandle<VoxelChunk>(chunk, handle);
                    OnChunkLoaded(new ChunkLoadedArgs(chunkId, chunkHadle));
                }

                current = next;
            }
        }

        private void ProcessLoadQueue(int requests)
        {
            while (requests > 0 && _chunkLoadRequests.Count > 0 && _maxRequestsGenerating > _chunksGenerating.Count)
            {
                var request = _chunkLoadRequests.Dequeue();
                requests--;

                switch (request.Type)
                {
                    case LoadRequestType.Loading:
                        LoadChunk(request.ChunkId);
                        break;
                    case LoadRequestType.Unloading:
                        UnloadChunk(request.ChunkId);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void LoadChunk(ChunkIdentity chunkId)
        {
            if (_chunksLoaded.ContainsKey(chunkId))
            {
                Debug.LogWarning($"Chunk ({chunkId}) is already loaded!");
                return;
            }

            var chunk = new VoxelChunk(_chunkSize);
            var loaded = false;
            // Try Load from disk
            if (_universeChunkIo.TryLoad(chunkId, out var loadedChunk))
            {
                // Copy from disk
                try
                {
                    loadedChunk.CopyTo(chunk);
                    loadedChunk.Dispose();
                    var handle = _chunksLoaded[chunkId] = new PersistentDataHandle<VoxelChunk>(chunk, new JobHandle());
                    OnChunkLoaded(new ChunkLoadedArgs(chunkId, handle));
                    loaded = true;
                }
                //On failure try to generate
                catch (ArgumentException argumentException)
                {
                    Debug.LogWarning($"Failed to copy data! Chunk size mismatch?\n{argumentException.StackTrace}");
                    // chunk.Dispose(); //It failed, so we dispose
                    loadedChunk.Dispose();

                }
            }
            
            if(!loaded)
            {
                var handle = _universeChunkGenerator.Generate(chunkId, chunk);
                var data = new Tuple<ChunkIdentity, VoxelChunk>(chunkId, chunk);
                var dataHandle = new DataHandle<Tuple<ChunkIdentity, VoxelChunk>>(data, handle);
                _chunksGenerating.AddLast(dataHandle);
//                _universeManager.ChunkMeshManager.RequestRender(chunkId,chunk,handle);
            }
        }

        private void UnloadChunk(ChunkIdentity chunkId)
        {
            if (!_chunksLoaded.TryGetValue(chunkId, out var chunkHandle))
            {
                Debug.LogWarning($"Chunk ({chunkId}) is not loaded!");
                return;
            }


            chunkHandle.Handle.Complete();
            _universeManager.ChunkMeshManager.RequestHide(chunkId);
            if (_universeChunkIo.TrySave(chunkId, chunkHandle.Data))
            {
                chunkHandle.Data.Dispose();
                _chunksLoaded.Remove(chunkId);
                OnChunkUnloaded(chunkId);
            }
        }


        public void ProcessRequests(int maxRequests = 1)
        {
            //Check generating
            CheckGeneratingQueue();

            ProcessLoadQueue(maxRequests);
        }


        [Obsolete("Use TryGetChunkHandle")]
        public bool TryGetChunk(ChunkIdentity chunkId, out VoxelChunk chunk)
        {
            if (TryGetChunkHandle(chunkId, out var chunkHandle))
            {
                chunk = chunkHandle.Data;
                return true;
            }

            chunk = default;
            return false;
        }

        public bool TryGetChunkHandle(ChunkIdentity chunkId, out PersistentDataHandle<VoxelChunk> chunkHandle) =>
            _chunksLoaded.TryGetValue(chunkId, out chunkHandle);


        public DirectionalNeighborhood<PersistentDataHandle<VoxelChunk>> GetChunkNeighborhood(ChunkIdentity chunkId)
        {
            var neighborhood = new DirectionalNeighborhood<PersistentDataHandle<VoxelChunk>>();
            PersistentDataHandle<VoxelChunk> temp;
            if (TryGetChunkHandle(chunkId, out temp))
                neighborhood.Center = temp;
            foreach (var dir in DirectionsX.AllDirections)
            {
                var dirChunkId = new ChunkIdentity(chunkId.World, chunkId.Chunk + dir.ToInt3());

                if (TryGetChunkHandle(dirChunkId, out temp))
                    neighborhood.SetNeighbor(dir, temp);
            }

            return neighborhood;
        }


        public event EventHandler<ChunkLoadedArgs> ChunkLoaded;

        public event EventHandler<ChunkIdentity> ChunkUnloaded;


        protected virtual void OnChunkLoaded(ChunkLoadedArgs args)
        {
            ChunkLoaded?.Invoke(this, args);
        }

        protected virtual void OnChunkUnloaded(ChunkIdentity chunkId)
        {
            ChunkUnloaded?.Invoke(this, chunkId);
        }
    }
}