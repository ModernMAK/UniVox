using UnityEngine;
using UniVox.Types;

namespace UniVox.Unity
{
    public class UniverseManager : MonoBehaviour
    {
        public static UniverseManager Instance { get; private set; }
        
        
        [SerializeField] private UniverseChunkManager _chunkManager;
        [SerializeField] private ChunkMeshManager _chunkMeshManager;
        [SerializeField] private ChunkGameObjectManager _chunkGoManager;


        public UniverseChunkManager ChunkManager => _chunkManager;
        public ChunkMeshManager ChunkMeshManager => _chunkMeshManager;
        public ChunkGameObjectManager ChunkGameObjectManager => _chunkGoManager;


        private void Awake()
        {
            Instance = this;
            
            if (_chunkManager == null)
                _chunkManager = GetComponentInChildren<UniverseChunkManager>();

            _chunkManager.InitializeManager(this);

            if (_chunkMeshManager == null)
                _chunkMeshManager = GetComponentInChildren<ChunkMeshManager>();
            _chunkMeshManager.InitializeManager(this);

            if (_chunkGoManager == null)
                _chunkGoManager = GetComponentInChildren<ChunkGameObjectManager>();
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