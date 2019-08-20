using System;
using DefaultNamespace;
using ECS.Voxel.Data;
using Unity.Collections;


public class Chunk : IDisposable
{
    public Chunk()
    {
        HiddenFaces = new NativeArray<Directions>(FlatSize, Allocator.Persistent);
        Shapes = new NativeArray<BlockShape>(FlatSize, Allocator.Persistent);
        Rotations = new NativeArray<Orientation>(FlatSize, Allocator.Persistent);
        SolidTable = new BitArray512().SetAll(true);
        BlockIds = new NativeArray<byte>(FlatSize, Allocator.Persistent);
    }

    public NativeArray<Orientation> Rotations { get; }
    public NativeArray<Directions> HiddenFaces { get; }
    public NativeArray<BlockShape> Shapes { get; }
    public BitArray512 SolidTable { get; }
    public NativeArray<byte> BlockIds { get; }

    public const int FlatSize = AxisSize * AxisSize * AxisSize;
    public const int AxisSize = 8;


    public void Dispose()
    {
        Rotations.Dispose();
        HiddenFaces.Dispose();
        Shapes.Dispose();
        BlockIds.Dispose();
    }
}