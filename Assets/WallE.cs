using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEdits.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

public class WallE : MonoBehaviour
{
    public enum ChunkSize
    {
        Byte = 1 << 2,
        Short = 1 << 5,
        Int = 1 << 10,
    }

    public ChunkSize SizeType;
    public int Size => (int) SizeType;

    public int UniverseSize = 0;

    public GameObject Prefab;

    [SerializeField] private Mesh _defaultMesh;
    [SerializeField] private Material _defaultMaterial;

    private World disposable;

    private class SystemUnloader
    {
        private ComponentSystemGroup initGroup;
        private ComponentSystemGroup updateGroup;
        private ComponentSystemGroup presentGroup;
        private World world;

        public SystemUnloader(World world)
        {
            this.world = world;
            initGroup = world.GetExistingSystem<InitializationSystemGroup>();
            updateGroup = world.GetExistingSystem<SimulationSystemGroup>();
            presentGroup = world.GetExistingSystem<PresentationSystemGroup>();
        }

        public void Unload<T>() where T : ComponentSystemBase
        {
            var system = world.GetExistingSystem<T>();
            if (system == null) return;
            initGroup.RemoveSystemFromUpdateList(system);
            updateGroup.RemoveSystemFromUpdateList(system);
            presentGroup.RemoveSystemFromUpdateList(system);
        }
    }


    int FlatSize => Size * Size * Size;

    void GenerateChunk(int3 chunkPos, EntityManager em, Entity prefab)
    {
        var chunkPosComp = new ChunkPosition() {Position = chunkPos};
        using (var array = new NativeArray<Entity>(FlatSize, Allocator.TempJob))
        {
            em.Instantiate(prefab, array);
            em.AddComponent<Static>(array);
            for (var x = 0; x < Size; x++)
            for (var y = 0; y < Size; y++)
            for (var z = 0; z < Size; z++)
            {
                var i = x + y * Size + z * Size * Size;
                em.SetComponentData(array[i], new Translation() {Value = new float3(x, y, z)});
                em.SetSharedComponentData(array[i], chunkPosComp);
                if (x != 0 && y != 0 && z != 0 && x != Size - 1 && y != Size - 1 && z != Size - 1)
                {
                    em.AddComponent<DontRenderTag>(array[i]);
                }

//                em.SetComponentData(array[i], new Rotation() {Value = Random.rotation});
            }
        }
    }

    void GenerateUniverse(EntityManager em, Entity prefab)
    {
        for (var x = -UniverseSize; x <= UniverseSize; x++)
        for (var y = -UniverseSize; y <= UniverseSize; y++)
        for (var z = -UniverseSize; z <= UniverseSize; z++)
        {
            GenerateChunk(new int3(x, y, z), em, prefab);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GameManager.MasterRegistry.Mesh.Register("Fallback", _defaultMesh);
        GameManager.MasterRegistry.Material.Register("Fallback", _defaultMaterial);

        var world = World.Active; //        new World("Real World");
//        world.EntityManager.CompleteAllJobs();

        var unloader = new SystemUnloader(world);

        unloader.Unload<RenderMeshSystemV4>();
        
//        unloader.Unload<RenderMeshSystemV3>();
//        unloader.Unload<LodRequirementsUpdateSystemV3>();
        
        
        unloader.Unload<RenderMeshSystemV2>();
        unloader.Unload<LodRequirementsUpdateSystem>();
        
        unloader.Unload<VoxelMeshSystemV1>();
        unloader.Unload<VoxelMeshSystemV2>();
        unloader.Unload<VoxelMeshSystemV3>();


//        ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world);
        var em = world.EntityManager;
        var prefab =
            GameObjectConversionUtility.ConvertGameObjectHierarchy(Prefab,
                new GameObjectConversionSettings(world, default));
        GenerateUniverse(em, prefab);

        em.DestroyEntity(prefab);
//        world.GetOrCreateSystem<RenderMeshSystemV3>();
//        world.GetOrCreateSystem<RenderMeshSystemV3>();
        disposable = world;
    }

    private void OnApplicationQuit()
    {
        disposable?.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
    }
}