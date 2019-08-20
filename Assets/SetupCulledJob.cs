using ECS.Voxel.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

internal struct SetupCulledJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Directions> HiddenFaces;
    [ReadOnly] public NativeArray<Entity> Entities;

    public EntityCommandBuffer.Concurrent Buffer;

    public void Execute(int index)
    {
        var e = Entities[index];
        var culling = HiddenFaces[index].IsAll();
        if (culling)
            Buffer.AddComponent<Disabled>(index, e);
        else
            Buffer.RemoveComponent<Disabled>(index, e);
    }
}