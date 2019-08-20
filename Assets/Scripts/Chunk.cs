using System;
using System.Collections.Generic;
using Types;
using Types.Native;
using Unity.Collections;
using Unity.Mathematics;

public class Chunk : IDisposable
{
    public const int FlatSize = AxisSize * AxisSize * AxisSize;
    public const int AxisSize = 8;

    public Chunk()
    {
        var allocator = Allocator.Persistent;
        HiddenFaces = new NativeArray<Directions>(FlatSize, allocator);
        Shapes = new NativeArray<BlockShape>(FlatSize, allocator);
        Rotations = new NativeArray<Orientation>(FlatSize, allocator);
        SolidFlags = new NativeArray<bool>(FlatSize, allocator);
        BlockIds = new NativeArray<byte>(FlatSize, allocator);
    }

    public NativeArray<Orientation> Rotations;
    public NativeArray<Directions> HiddenFaces;
    public NativeArray<BlockShape> Shapes;

    public NativeArray<bool> SolidFlags;
    public NativeArray<byte> BlockIds { get; }


    public void Dispose()
    {
        Rotations.Dispose();
        HiddenFaces.Dispose();
        Shapes.Dispose();
        BlockIds.Dispose();
        SolidFlags.Dispose();
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