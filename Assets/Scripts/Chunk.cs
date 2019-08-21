using System;
using Types;
using Unity.Collections;


public class Chunk : IDisposable
{
    public const int FlatSize = AxisSize * AxisSize * AxisSize;
    public const int AxisSize = 8;
    public NativeArray<Directions> HiddenFaces;
    public NativeArray<Orientation> Rotations;
    public NativeArray<BlockShape> Shapes;
    public NativeArray<bool> ActiveFlags;
    
    public Chunk()
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


//public struct NativeWorld : IDisposable
//{
//    public NativeWorld(Allocator allocator)
//    {
//        NativeChunkTable = new NativeHashMap<int3, int>(byte.MaxValue, allocator);
//    }
//
//    public NativeHashMap<int3, int> NativeChunkTable { get; }
//
//    public void Dispose()
//    {
//        NativeChunkTable.Dispose();
//    }
//}