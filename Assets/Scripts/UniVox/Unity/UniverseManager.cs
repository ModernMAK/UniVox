using UnityEngine;
using UniVox.Types;

namespace UniVox.Unity
{
    public class UniverseManager : MonoBehaviour
    {
        [SerializeField] private UniverseChunkManager _chunkManager;
        [SerializeField] private ChunkMeshManager _chunkMeshManager;


        public UniverseChunkManager ChunkManager => _chunkManager;
        public ChunkMeshManager ChunkMeshManager => _chunkMeshManager;


        private void Awake()
        {
            if (_chunkManager == null)
                _chunkManager = GetComponentInChildren<UniverseChunkManager>();

            _chunkManager.InitializeManager(this);

            if (_chunkMeshManager == null)
                _chunkMeshManager = GetComponentInChildren<ChunkMeshManager>();
            _chunkMeshManager.InitializeManager(this);
        }

        private void OnEnable()
        {
            ChunkManager.ChunkLoaded += ChunkLoaded;
            ChunkManager.ChunkUnloaded += ChunkUnloaded;
        }

        private void OnDisable()
        {
            ChunkManager.ChunkLoaded -= ChunkLoaded;
            ChunkManager.ChunkUnloaded -= ChunkUnloaded;
        }

        private void ChunkLoaded(object sender, ChunkLoadedArgs args)
        {
            var neighborhood = ChunkManager.GetChunkNeighborhood(args.Identity);
            ChunkMeshManager.RequestRender(args.Identity, args.ChunkHandle);
            foreach (var dir in DirectionsX.AllDirections)
            {
                var neighbor = neighborhood.GetNeighbor(dir);
                if (neighbor != null)
                {
                    var neighborId = new ChunkIdentity(args.Identity.World, args.Identity.Chunk + dir.ToInt3());
                    ChunkMeshManager.RequestRender(neighborId, neighbor);
                }
            }
        }

        private void ChunkUnloaded(object sender, ChunkIdentity args)
        {
            ChunkMeshManager.RequestHide(args);
        }
    }
}