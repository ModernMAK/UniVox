//using System;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Mathematics;
//
///// <summary>
///// Represents a table lookup for chunks
///// </summary>
//public struct OldChunkTable : ISharedComponentData, IDisposable, IEquatable<OldChunkTable>
//{
//    public NativeHashMap<int3, Entity> value;
//
//    public void Dispose()
//    {
//        value.Dispose();
//    }
//
//    public bool Equals(OldChunkTable other)
//    {
//        return value.Equals(other.value);
//    }
//
//    public override bool Equals(object obj)
//    {
//        return obj is OldChunkTable other && Equals(other);
//    }
//
//    public override int GetHashCode()
//    {
//        return value.GetHashCode();
//    }
//}
//
//
/////// <summary>
/////// Represents a table lookup for chunks
/////// </summary>
////public struct ChunkInfo : ISharedComponentData, IDisposable, IEquatable<OldChunkTable>
////{
////    public NativeArray<Entity> value;
////    public int3 size;
////
////    public static int ConvertToIndex(int3 position, int3 size)
////    {
////        
////    }
////
////    public static int3 ConvertFromIndex(int index, int3 size)
////    {
////        
////    }
////
////    public void Dispose()
////    {
////        value.Dispose();
////    }
////
////    public bool Equals(OldChunkTable other)
////    {
////        return value.Equals(other.value);
////    }
////
////    public override bool Equals(object obj)
////    {
////        return obj is OldChunkTable other && Equals(other);
////    }
////
////    public override int GetHashCode()
////    {
////        return value.GetHashCode();
////    }
////}