using ECS.Voxel.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
internal struct SetupVisibilityJob : IJobParallelFor
{
    [WriteOnly] public NativeArray<Directions> HiddenFaces;

    public void Execute(int index)
    {
        var position = new VoxelPos32(index).Position;
        var flags = DirectionsX.NoneFlag;

        if (position.x != 0)
            flags |= Directions.Left;
        if (position.x != VoxelPos32.MaxValue)
            flags |= Directions.Right;

        if (position.y != 0)
            flags |= Directions.Down;
        if (position.y != VoxelPos32.MaxValue)
            flags |= Directions.Up;

        if (position.z != 0)
            flags |= Directions.Backward;
        if (position.z != VoxelPos32.MaxValue)
            flags |= Directions.Forward;


        HiddenFaces[index] = flags;
    }
}