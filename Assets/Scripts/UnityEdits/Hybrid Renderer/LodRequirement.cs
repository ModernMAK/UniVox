using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace UnityEdits.Rendering
{
    struct LodRequirement : IComponentData
    {
        public float3 WorldReferencePosition;
        public float MinDist;
        public float MaxDist;

        public LodRequirement(MeshLODGroupComponent lodGroup, LocalToWorld localToWorld, int lodMask)
        {
            var referencePoint = math.transform(localToWorld.Value, lodGroup.LocalReferencePoint);
            float minDist = float.MaxValue;
            float maxDist = 0.0F;
            if ((lodMask & 0x01) == 0x01)
            {
                minDist = 0.0f;
                maxDist = math.max(maxDist, lodGroup.LODDistances0.x);
            }

            if ((lodMask & 0x02) == 0x02)
            {
                minDist = math.min(minDist, lodGroup.LODDistances0.x);
                maxDist = math.max(maxDist, lodGroup.LODDistances0.y);
            }

            if ((lodMask & 0x04) == 0x04)
            {
                minDist = math.min(minDist, lodGroup.LODDistances0.y);
                maxDist = math.max(maxDist, lodGroup.LODDistances0.z);
            }

            if ((lodMask & 0x08) == 0x08)
            {
                minDist = math.min(minDist, lodGroup.LODDistances0.z);
                maxDist = math.max(maxDist, lodGroup.LODDistances0.w);
            }

            if ((lodMask & 0x10) == 0x10)
            {
                minDist = math.min(minDist, lodGroup.LODDistances0.w);
                maxDist = math.max(maxDist, lodGroup.LODDistances1.x);
            }

            if ((lodMask & 0x20) == 0x20)
            {
                minDist = math.min(minDist, lodGroup.LODDistances1.x);
                maxDist = math.max(maxDist, lodGroup.LODDistances1.y);
            }

            if ((lodMask & 0x40) == 0x40)
            {
                minDist = math.min(minDist, lodGroup.LODDistances1.y);
                maxDist = math.max(maxDist, lodGroup.LODDistances1.z);
            }

            if ((lodMask & 0x80) == 0x80)
            {
                minDist = math.min(minDist, lodGroup.LODDistances1.z);
                maxDist = math.max(maxDist, lodGroup.LODDistances1.w);
            }

            WorldReferencePosition = referencePoint;
            MinDist = minDist;
            MaxDist = maxDist;
        }
    }
}