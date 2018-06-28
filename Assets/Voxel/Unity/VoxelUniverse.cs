using System;
using UnityEngine;
using Voxel.Core;

namespace Voxel.Unity
{
    public class VoxelUniverse : MonoBehaviour
    {
        public Int3 InitChunks;
        public Int3 ChunkSize;
        public Universe Universe { get; private set; }
        private void Awake()
        {
            Universe = new Universe(ChunkSize);
            foreach (var pos in Int3.RangeEnumerable(-InitChunks, InitChunks, true))
            {
                Universe.RequestChunk(pos,true);
            }
        }

        private void Update()
        {
            Universe.Update();
        }

        public static Int3 Convert(Vector3 position)
        {
            return Int3.Floor(position);
        }

        public static Vector3 FixOffset(Int3 position)
        {
            return FixOffset((Vector3) position);
        }

        public static Vector3 FixOffset(Vector3 position)
        {
            return position + Vector3.one / 2f;
        }
    }
}