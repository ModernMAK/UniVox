using Unity.Mathematics;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace ECS.System
{
    public class PhysicsUtil
    {
        public static CollisionWorld GetActiveCollisionWorld()
        {
            var physicsWorldSystem = World.Active.GetExistingSystem<BuildPhysicsWorld>();
            var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
            return collisionWorld;
        }

        public static bool Raycast(RaycastInput ray, out RaycastHit hit, CollisionWorld world)
        {
            return world.CastRay(ray, out hit);
        }

        public static RaycastInput GetRay(float3 from, float3 to, uint rayLayers = ~0u, uint targetLayers = ~0u,
            int groupIndex = 0)
        {
            return new RaycastInput()
            {
                Start = from,
                End = to,
                Filter = new CollisionFilter()
                {
                    BelongsTo = rayLayers, // all 1s, so all layers, collide with everything 
                    CollidesWith = targetLayers,
                    GroupIndex = groupIndex
                }
            };
        }

        public struct EntityHit
        {
            public Entity entity;
            public RaycastHit hit;
        }

        public static bool RaycastEntity(float3 RayFrom, float3 RayTo, out EntityHit hitEntity)
        {
            var world = GetActiveCollisionWorld();
            var ray = GetRay(RayFrom, RayTo);

            if (Raycast(ray, out var hit, world))
            {
                // see hit.Position 
                // see hit.SurfaceNormal
                hitEntity = new EntityHit()
                {
                    entity = world.Bodies[hit.RigidBodyIndex].Entity,
                    hit = hit
                };
                return true;
            }

            hitEntity = new EntityHit()
            {
                entity = Entity.Null,
                hit = hit
            };
            return false;
        }
    }
}