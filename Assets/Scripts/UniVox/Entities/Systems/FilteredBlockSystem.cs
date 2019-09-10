using Unity.Entities;
using Unity.Jobs;
using Univox.Entities.Data;

public abstract class FilteredBlockSystem : JobComponentSystem
{
    protected override void OnCreateManager()
    {
        var desc = new EntityQueryDesc()
        {
            All = new[] {ComponentType.ReadOnly<BlockIdentity>(),},
        };
        var q = GetEntityQuery(desc);
        ApplyBlockFilter(q);
    }

    protected EntityQuery ApplyBlockFilter(EntityQuery query)
    {
        query.SetFilter(GetBlockIdentity());
        return query;
    }

    protected abstract BlockIdentity GetBlockIdentity();

    protected abstract override JobHandle OnUpdate(JobHandle inputDependencies);
}

/* 
 * Registry => Meshes
 * Registry => Textures (?)
 * Registry => Materials
 * Registry => 
 */