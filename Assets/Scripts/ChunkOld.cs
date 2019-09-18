using System;
using Types;
using Unity.Collections;


[Obsolete]
public class ChunkOld : IDisposable
{
    public const int FlatSize = AxisSize * AxisSize * AxisSize;
    public const int AxisSize = 8;
    public NativeArray<bool> ActiveFlags;
    public NativeArray<Directions> HiddenFaces;
    public NativeArray<Orientation> Rotations;
    public NativeArray<BlockShape> Shapes;

    public ChunkOld()
    {
        var allocator = Allocator.Persistent;
        HiddenFaces = new NativeArray<Directions>(FlatSize, allocator);
        Shapes = new NativeArray<BlockShape>(FlatSize, allocator);
        Rotations = new NativeArray<Orientation>(FlatSize, allocator);
        ActiveFlags = new NativeArray<bool>(FlatSize, allocator);
        BlockIds = new NativeArray<byte>(FlatSize, allocator);
    }

    public NativeArray<byte> BlockIds { get; }


    public void Dispose()
    {
        Rotations.Dispose();
        HiddenFaces.Dispose();
        Shapes.Dispose();
        BlockIds.Dispose();
        ActiveFlags.Dispose();
    }
}
