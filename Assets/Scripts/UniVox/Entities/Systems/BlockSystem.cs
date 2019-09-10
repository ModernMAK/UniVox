using System.IO;
using InventorySystem;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
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

public abstract class BlockRenderSystem : JobComponentSystem
{
}

public class MasterRegistry
{
    public MasterRegistry()
    {
        Mesh = new NamedRegistry<Mesh>();
        Material = new NamedRegistry<Material>();
        Icon = new NamedRegistry<Sprite>();
    }

    public NamedRegistry<Mesh> Mesh { get; }
    public NamedRegistry<Material> Material { get; }
    public NamedRegistry<Sprite> Icon { get; }
}

public class ModInitializer
{
    public ModInitializer(MasterRegistry registries)
    {
        Registries = registries;
    }

    public MasterRegistry Registries { get; }
}

public abstract class AbstractMod
{
    public abstract void Initialize(ModInitializer initializer);
}

public static class ModAssets
{
    private static T Load<T>(string name, string modPath, string folderPath) where T : Object
    {
        return Resources.Load<T>(Path.Combine(modPath, folderPath, name));
    }

    public static T Load<T>(string name, string fullResourcePath) where T : Object
    {
        return Resources.Load<T>(Path.Combine(fullResourcePath, name));
    }

    public static Mesh LoadMesh(string name, string modPath = default)
    {
        const string assetFolder = "Meshes";
        return Load<Mesh>(name, modPath, assetFolder);
    }

    public static bool LoadAndRegisterMesh<TKey>(this IRegistry<TKey, Mesh> registry, TKey key, string resourceName,
        out Mesh mesh, string modPath = default)
    {
        if (!registry.IsRegistered(key))
        {
            mesh = LoadMesh(resourceName, modPath);
            registry.Register(key, mesh);
            return true;
        }

        mesh = default;
        return false;
    }

    public static bool LoadAndRegisterMesh<TKey>(this IRegistry<TKey, Mesh> registry, TKey key, string resourceName,
        string modPath = default)
    {
        return registry.LoadAndRegisterMesh(key, resourceName, out _, modPath);
    }

    public static Material LoadMaterial(string name, string modPath = default)
    {
        const string assetFolder = "Materials";
        return Load<Material>(name, modPath, assetFolder);
    }

    public static bool LoadAndRegisterMaterial<TKey>(this IRegistry<TKey, Material> registry, TKey key,
        string resourceName,
        out Material material, string modPath = default)
    {
        if (!registry.IsRegistered(key))
        {
            material = LoadMaterial(resourceName, modPath);
            registry.Register(key, material);
            return true;
        }

        material = default;
        return false;
    }
    public static bool LoadAndRegisterMaterial<TKey>(this IRegistry<TKey, Material> registry, TKey key, string resourceName,
        string modPath = default)
    {
        return registry.LoadAndRegisterMaterial(key, resourceName, out _, modPath);
    }

    public static Sprite LoadSprite(string name, string modPath = default)
    {
        const string assetFolder = "Sprites";
        return Load<Sprite>(name, modPath, assetFolder);
    }

    public static Texture LoadTexture(string name, string modPath = default)
    {
        const string assetFolder = "Textures";
        return Load<Texture>(name, modPath, assetFolder);
    }

    public static Texture LoadTexture2D(string name, string modPath = default)
    {
        const string assetFolder = "Textures";
        return Load<Texture2D>(name, modPath, assetFolder);
    }

    public static Texture LoadTexture3D(string name, string modPath = default)
    {
        const string assetFolder = "Textures";
        return Load<Texture3D>(name, modPath, assetFolder);
    }
}

public class BaseGameMod : AbstractMod
{
    public override void Initialize(ModInitializer initializer)
    {
        var meshReg = initializer.Registries.Mesh;
        meshReg.LoadAndRegisterMesh("Block", "Cube", "BaseGame");
    }
}

/* 
 * Registry => Meshes
 * Registry => Textures (?)
 * Registry => Materials
 * Registry => 
 */