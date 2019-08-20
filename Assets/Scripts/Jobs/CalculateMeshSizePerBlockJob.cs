using System;
using Types;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Jobs
{
    
    /// <summary>
    /// Calculates the Mesh Size on a Per Block Basis, each index in Vertexes and Triangles corresponds to the Block at that index
    /// Vertexes will be [0,24], and Triangles will be [0,36]
    /// </summary>
    [BurstCompile]
    public struct CalculateMeshSizePerBlockJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Directions> HiddenFaces;
        [ReadOnly] public NativeArray<BlockShape> Shapes;
        [ReadOnly] public NativeArray<Orientation> Rotations;
        [WriteOnly] public NativeArray<int> Vertexes;
        [WriteOnly] public NativeArray<int> Triangles;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Direction> Directions;


        private void CalculateCube(Directions hidden, out int verts, out int indexes)
        {
            var quads = 0;
            for (var i = 0; i < 6; i++)
                if (!hidden.HasDirection(Directions[i]))
                    quads++;

            verts = quads * 4;
            indexes = quads * 2 * 3;
        }

        public void Execute(int index)
        {
            var shape = Shapes[index];
            var hidden = HiddenFaces[index];
            var rotation = Rotations[index];

            var verts = 0;
            var indexes = 0;
            switch (shape)
            {
                case BlockShape.Cube:
                    CalculateCube(hidden, out verts, out indexes);
                    break;
                case BlockShape.CornerInner:
                    break;
                case BlockShape.CornerOuter:
                    break;
                case BlockShape.Ramp:
                    break;
                case BlockShape.CubeBevel:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Vertexes[index] = verts;
            Triangles[index] = indexes;
        }
    }
}