using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEdits.Rendering;
using UnityEngine;


public class WallE2 : MonoBehaviour
{
    public enum ChunkSize
    {
        Byte = 1 << 2, //8/3 ~ 2
        Short = 1 << 5, //16/3 ~ 5
        Int = 1 << 10, //32/3 ~ 10
    }

    public ChunkSize SizeType;
    public int Size => (int) SizeType;

    public int UniverseSize = 0;

//    public GameObject Prefab;

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

    void CreateChunk(int3 chunkPos, EntityManager em, EntityArchetype archetype)
    {
        var entity = em.CreateEntity(archetype);
        em.SetComponentData(entity, new VoxelChunk(FlatSize));
        var renderData = new VoxelRenderChunk(FlatSize);
        em.SetComponentData(entity, renderData);
        em.SetComponentData(entity, new VoxelChunkPosition() {Value = chunkPos});

        //Iterate over all internal bits
        for (var x = 1; x < Size - 1; x++)
        for (var y = 1; x < Size - 1; y++)
        for (var z = 1; x < Size - 1; z++)
        {
            var index = x + y * Size + z * Size * Size;
            renderData.ShouldCullFlag[index] = true;
        }
    }


    void GenerateUniverse(EntityManager em, EntityArchetype archetype)
    {
        for (var x = -UniverseSize; x <= UniverseSize; x++)
        for (var y = -UniverseSize; y <= UniverseSize; y++)
        for (var z = -UniverseSize; z <= UniverseSize; z++)
        {
            CreateChunk(new int3(x, y, z), em, archetype);
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


//        ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world);
        var em = world.EntityManager;
        var archetype = em.CreateArchetype(typeof(VoxelRenderChunk), typeof(VoxelChunk), typeof(ChunkPosition));
//        var prefab =
//            GameObjectConversionUtility.ConvertGameObjectHierarchy(Prefab,
//                new GameObjectConversionSettings(world, default));
        GenerateUniverse(em,archetype);

//        em.DestroyEntity(prefab);
//        world.GetOrCreateSystem<RenderMeshSystemV3>();
//        world.GetOrCreateSystem<RenderMeshSystemV3>();
        disposable = world;
    }

    private void OnApplicationQuit()
    {
        disposable?.Dispose();
    }

}