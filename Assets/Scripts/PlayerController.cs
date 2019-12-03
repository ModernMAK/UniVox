using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions.Must;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class PlayerController : MonoBehaviour, IConvertGameObjectToEntity
{
    // Add fields to your component here. Remember that:
    //
    // * The purpose of this class is to store data for authoring purposes - it is not for use while the game is
    //   running.
    // 
    // * Traditional Unity serialization rules apply: fields must be public or marked with [SerializeField], and
    //   must be one of the supported types.
    //
    // For example,
    //    public float scale;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // Call methods on 'dstManager' to create runtime components on 'entity' here. Remember that:
        //
        // * You can add more than one component to the entity. It's also OK to not add any at all.
        //
        // * If you want to create more than one entity from the data in this class, use the 'conversionSystem'
        //   to do it, instead of adding entities through 'dstManager' directly.
        //
        // For example,
        //   dstManager.AddComponentData(entity, new Unity.Transforms.Scale { Value = scale });
        dstManager.AddComponent(entity, typeof(Player));
    }
}

//TAG COMPONENT
public struct Player : IComponentData
{
}

public class PlayerControllerSystem : JobComponentSystem
{
    private EntityQuery _query;

    protected override void OnCreate()
    {
        _query = GetEntityQuery(new EntityQueryDesc()
        {
            All = new[]
            {
                typeof(Player),
                ComponentType.ReadWrite<PhysicsVelocity>(),
                ComponentType.ReadWrite<WorldToLocal>()
            }
        });
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
        const float s = 8f;
        var job = new ApplyPlanarVelocity(h, v, s);
        return job.Schedule(_query, inputDeps);
    }

    public struct ApplyPlanarVelocity : IJobForEach<PhysicsVelocity, WorldToLocal>
    {
        private readonly float3 _planerVelocity;

        public ApplyPlanarVelocity(float horizontal, float vertical, float speed)
        {
            var planerVel = new Vector3(horizontal, 0, vertical) * speed;
            if (planerVel.sqrMagnitude > speed * speed)
                planerVel = planerVel.normalized * speed;
            _planerVelocity = planerVel;
        }

        public void Execute(ref PhysicsVelocity physicsVelocity, ref WorldToLocal worldToLocal)
        {
            var yVel = physicsVelocity.Linear.y;
            var velocity = math.rotate(worldToLocal.Value, _planerVelocity) + new float3(0, yVel, 0);
            physicsVelocity.Linear = velocity;
            physicsVelocity.Angular = float3.zero;
        }
    }
}