using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UniVox.Rendering.ChunkGen.Jobs
{
    [BurstCompile]

    public struct CalculateCubeSizeJobV2 : IJobParallelFor
    {
        /// <summary>
        ///     An array reperesenting the indexes to process
        ///     This is useful for seperating blocks with different materials.
        /// </summary>
        [ReadOnly] public NativeArray<PlanarData> PlanarInBatch;

//        /// <summary>
//        ///     The Chunk's Shape Array
//        /// </summary>
//        [ReadOnly] public NativeArray<BlockShape> Shapes;
//
//        /// <summary>
//        ///     The Chunk's Hidden Faces Array
//        /// </summary>
//        [ReadOnly] public NativeArray<Directions> HiddenFaces;

        /// <summary>
        ///     The Vertex Sizes, should be the same length as Batch Indexes
        /// </summary>
        [WriteOnly] [NativeDisableParallelForRestriction]
        public NativeArray<int> VertexSizes;

        /// <summary>
        ///     The INdex Sizes, should be the same length as Batch Indexes
        /// </summary>
        [WriteOnly] [NativeDisableParallelForRestriction]
        public NativeArray<int> TriangleSizes;

        /// <summary>
        ///     An array representing the six possible directions. Provided to avoid creating and destroying it over and over again
        /// </summary>
//        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Direction> Directions;

        //Obvious Constants, but they are easier to read than Magic Numbers
        private const int QuadSize = 4;

        private const int QuadIndexSize = 6;

        private const int TriSize = 3;
        private const int TriIndexSize = 3;


//        private void CalculateCube(int position)
//        {
//            var hidden = HiddenFaces[position];
//            var vertSize = 0;
//            var indexSize = 0;
//            for (var i = 0; i < Directions.Length; i++)
//            {
//                if (hidden.HasDirection(Directions[i])) continue;
//
//                vertSize += QuadSize;
//                indexSize += QuadIndexSize;
//            }
//
//            VertexSizes[position] = vertSize;
//            TriangleSizes[position] = indexSize;
//        }

        public void Execute(int index)
        {
            var planar = PlanarInBatch[index];
            var vertSize = 0;
            var indexSize = 0;
            vertSize += QuadSize;
            indexSize += QuadIndexSize;
            VertexSizes[index] = vertSize;
            TriangleSizes[index] = indexSize;
//            
//            var blockIndex = Batch[position];
//            switch (Shapes[blockIndex])
//            {
//                case BlockShape.Cube:
//                    CalculateCube(blockIndex);
//                    break;
//                case BlockShape.CornerInner:
//                case BlockShape.CornerOuter:
//                case BlockShape.Ramp:
//                case BlockShape.CubeBevel:
//                    throw new NotImplementedException();
//
//
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }
        }
    }
}