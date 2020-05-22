using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace UniVox.MeshGen
{
    public static class NativeColliderUtil
    {
        public static BlobAssetReference<Collider> Create(NativeArray<float3> vertexes,
            NativeArray<int3> indexes)
        {
            return MeshCollider.Create(vertexes, indexes);
        }

        public static BlobAssetReference<Collider> Create(NativeArray<float3> vertexes,
            NativeArray<int3> indexes, CollisionFilter filter)
        {
            return MeshCollider.Create(vertexes, indexes, filter);
        }


        public static BlobAssetReference<Collider> Create(NativeArray<float3> vertexes,
            NativeArray<int> indexes)
        {
            throw new NotImplementedException();
//            return MeshCollider.Create(vertexes, indexes);
        }

        public static BlobAssetReference<Collider> Create(NativeArray<float3> vertexes,
            NativeArray<int> indexes, CollisionFilter filter)
        {
            throw new NotImplementedException();
//            return MeshCollider.Create(vertexes, indexes, filter);
        }
    }
}