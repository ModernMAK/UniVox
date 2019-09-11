using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace UnityEdits.Rendering
{
    [BurstCompile]
    struct GatherVoxelRenderMatrixV2 : IJobParallelFor
    {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<LocalToWorld> LocalToWorlds;
        [WriteOnly] public NativeArray<float4x4> Matricies;

        [ReadOnly] public float3 MatrixOffset;


        public void Execute(int entityIndex)
        {
            var localToWorld = LocalToWorlds[entityIndex];
            var matrix = localToWorld.Value;
            //Position is c3 => xyz
            Matricies[entityIndex] = matrix - new float4x4(0, MatrixOffset);
        }
    }
}