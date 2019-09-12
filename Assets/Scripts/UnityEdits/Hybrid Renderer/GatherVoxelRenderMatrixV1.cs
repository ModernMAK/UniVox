using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnityEdits.Rendering
{
    [BurstCompile]
    internal struct GatherVoxelRenderMatrixV1 : IJobParallelFor
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<LocalToWorld> LocalToWorlds;
        [ReadOnly] public NativeArray<int3> offsets;
        [ReadOnly] public int offsetIndex;
        [WriteOnly] public NativeArray<float4x4> Matricies;

        public float3 MatrixOffset => offsets[offsetIndex];


        public void Execute(int entityIndex)
        {
            var localToWorld = LocalToWorlds[entityIndex];
            var matrix = localToWorld.Value;
            //Position is c3 => xyz
            Matricies[entityIndex] = matrix - new float4x4(0, MatrixOffset);
        }
    }
}