using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ECS.UniVox.VoxelChunk.Systems.ChunkJobs
{
    //TODO Depricate this, as it does nothing
    [BurstCompile]
    public struct CalculateCubeSizeJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<PlanarData> PlanarInBatch;

        [WriteOnly] [NativeDisableParallelForRestriction]
        public NativeArray<int> VertexSizes;

        [WriteOnly] [NativeDisableParallelForRestriction]
        public NativeArray<int> TriangleSizes;


        private const int QuadSize = 4;

        private const int QuadIndexSize = 6;

        private const int TriSize = 3;
        private const int TriIndexSize = 3;


        public void Execute(int index)
        {
            var planar = PlanarInBatch[index];
            var vertSize = 0;
            var indexSize = 0;
            vertSize += QuadSize;
            indexSize += QuadIndexSize;
            VertexSizes[index] = vertSize;
            TriangleSizes[index] = indexSize;
        }
    }

//    [BurstCompile]
//    public struct CalculateCubeSizeJob : IJobParallelFor
//    {
//        /// <summary>
//        ///     An array reperesenting the indexes to process
//        ///     This is useful for seperating blocks with different materials.
//        /// </summary>
//        [ReadOnly] public NativeList<PlanarData> PlanarInBatch;
//
//        /// <summary>
//        ///     The Vertex Sizes, should be the same length as Batch Indexes
//        /// </summary>
//        [WriteOnly] [NativeDisableParallelForRestriction]
//        public NativeList<int> VertexSizes;
//
//        /// <summary>
//        ///     The INdex Sizes, should be the same length as Batch Indexes
//        /// </summary>
//        [WriteOnly] [NativeDisableParallelForRestriction]
//        public NativeList<int> TriangleSizes;
//
//        /// <summary>
//        ///     An array representing the six possible directions. Provided to avoid creating and destroying it over and over again
//        /// </summary>
////        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Direction> Directions;
//
//        //Obvious Constants, but they are easier to read than Magic Numbers
//        private const int QuadSize = 4;
//
//        private const int QuadIndexSize = 6;
//
//        private const int TriSize = 3;
//        private const int TriIndexSize = 3;
//
//
//        public void Execute(int index)
//        {
//        }
//
//        public void Execute()
//        {
//            for (var index = 0; index < PlanarInBatch.Length; index++)
//            {
//                
//            }
//            var planar = PlanarInBatch[index];
//            var vertSize = 0;
//            var indexSize = 0;
//            vertSize += QuadSize;
//            indexSize += QuadIndexSize;
//            VertexSizes[index] = vertSize;
//            TriangleSizes[index] = indexSize;
//        }
//    }
}