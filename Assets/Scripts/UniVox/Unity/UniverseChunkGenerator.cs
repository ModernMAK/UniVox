using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Types;
using UniVox.WorldGen;

namespace UniVox.Unity
{
    public class UniverseChunkGenerator : MonoBehaviour
    {
        [SerializeField] private int _seed = 8675309;
        [Range(0f, 1f)] [SerializeField] private float _solidity = 1f;

        private AbstractGenerator<int3, VoxelChunk> _generator;

        public void Awake()
        {
            _generator = new VoxelChunkGenerator()
            {
                Seed = _seed,
                Solidity = _solidity
            };
        }


        public JobHandle Generate(ChunkIdentity chunkId, VoxelChunk chunk)
        {
            return _generator.Generate(chunkId.Chunk * chunk.ChunkSize, chunk);
        }
    }
}