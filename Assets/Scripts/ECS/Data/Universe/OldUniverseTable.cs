//using System;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Mathematics;
//
//public struct OldUniverseTable : ISharedComponentData, IDisposable, IEquatable<OldUniverseTable>
//{
//    public NativeHashMap<int3, Entity> value;
//
//    public void Dispose()
//    {
//        value.Dispose();
//    }
//
//    public bool Equals(OldUniverseTable other)
//    {
//        return value.Equals(other.value);
//    }
//
//    public override bool Equals(object obj)
//    {
//        return obj is OldUniverseTable other && Equals(other);
//    }
//
//    public override int GetHashCode()
//    {
//        return value.GetHashCode();
//    }
//}