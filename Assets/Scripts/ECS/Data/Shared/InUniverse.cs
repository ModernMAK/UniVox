//using System;
//using Unity.Entities;
//
//public struct InUniverse : ISharedComponentData, IEquatable<InUniverse>
//{
//    /// <summary>
//    /// The Entity representing the universe that this is in
//    /// </summary>
//    public Entity value;
//
//    public bool Equals(InUniverse other)
//    {
//        return value.Equals(other.value);
//    }
//
//    public override bool Equals(object obj)
//    {
//        return obj is InUniverse other && Equals(other);
//    }
//
//    public override int GetHashCode()
//    {
//        return value.GetHashCode();
//    }
//}

