using System;
using System.Runtime.Serialization;
using ECS.Voxel.Data;
using Unity.Collections;

public class ChunkData : ISerializable, IDisposable
{
    public NativeBitArray SolidTable; //((2^5)^3)/(2^3) -> (2^2)^3 -> 64 bytes
//32 ^ 3 ->1 32768 bytes


    public NativeArray<Directions> HiddenFaces;
//    public byte[,,] BlockTypeId;


    private const int FlatSize = AxisSize * AxisSize * AxisSize;
    private const int AxisSize = 8;

    public ChunkData()
    {
        SolidTable = new NativeBitArray(FlatSize, Allocator.Persistent);
        HiddenFaces = new NativeArray<Directions>(FlatSize, Allocator.Persistent);
    }

    public ChunkData(SerializationInfo info, StreamingContext context)
    {
        var bytes = info.GetValue<byte[]>("Solidity");
        var dirs = info.GetValue<Directions[]>("Visibility");
        throw new NotImplementedException();
    }

    public const int ChunkSizePerAxis = AxisSize;

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Version", 0);
        info.AddValue("Solidity", SolidTable);
        info.AddValue("Visibility", HiddenFaces);
    }

    public void Dispose()
    {
        SolidTable.Dispose();
        HiddenFaces.Dispose();
    }
}