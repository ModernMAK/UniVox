using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

internal struct InitBlockJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Entity> Entities;
    [ReadOnly] public float3 RenderOffset;
    [ReadOnly] public int3 ChunkOffset;
    public EntityCommandBuffer.Concurrent Buffer;

    public void Execute(int index)
    {
        var e = Entities[index];
        var voxPos = new VoxelPos32(index);
        Buffer.SetComponent(index, e, new Translation {Value = voxPos.Position + RenderOffset + ChunkOffset});
        Buffer.AddComponent<Disabled>(index, e);
    }
}