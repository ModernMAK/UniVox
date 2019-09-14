//using Unity.Collections;
//using Unity.Entities;
//using Unity.Mathematics;
//using Unity.Rendering;
//using Unity.Transforms;
//using UnityEdits.Rendering;
//using UnityEngine;
//
//
//public class WallE2 : MonoBehaviour
//{
//    public enum ChunkSize
//    {
//        Byte = 1 << 2, //8/3 ~ 2
//        Short = 1 << 5, //16/3 ~ 5
//        Int = 1 << 10, //32/3 ~ 10
//    }
//
//    public ChunkSize SizeType;
//    public int Size => (int) SizeType;
//
//    public int UniverseSize = 0;
//
////    public GameObject Prefab;
//
//    [SerializeField] private Mesh _defaultMesh;
//    [SerializeField] private Material _defaultMaterial;
//
//    private World disposable;
//
//    private class SystemUnloader
//    {
//        private ComponentSystemGroup initGroup;
//        private ComponentSystemGroup updateGroup;
//        private ComponentSystemGroup presentGroup;
//        private World world;
//
//        public SystemUnloader(World world)
//        {
//            this.world = world;
//            initGroup = world.GetExistingSystem<InitializationSystemGroup>();
//            updateGroup = world.GetExistingSystem<SimulationSystemGroup>();
//            presentGroup = world.GetExistingSystem<PresentationSystemGroup>();
//        }
//
//        public void Unload<T>() where T : ComponentSystemBase
//        {
//            var system = world.GetExistingSystem<T>();
//            if (system == null) return;
//            initGroup.RemoveSystemFromUpdateList(system);
//            updateGroup.RemoveSystemFromUpdateList(system);
//            presentGroup.RemoveSystemFromUpdateList(system);
//        }
//    }
//
//
//    int FlatSize => Size * Size * Size;
//
//    void CreateChunk(int3 chunkPos, EntityManager em, EntityArchetype archetype)
//    {
//        var entity = em.CreateEntity(archetype);
////        em.SetComponentData(entity, new VoxelChunk(FlatSize));
////        var renderData = new VoxelRenderChunkOld(FlatSize);
//
//        var temp = new NativeArray<VoxelRenderChunkElement>(FlatSize, Allocator.Temp,
//            NativeArrayOptions.UninitializedMemory);
//
////        em.SetComponentData(entity, renderData);
//        em.SetComponentData(entity, new VoxelChunkPosition() {Value = chunkPos});
//
//
//        const int trimOffset = 0;
//        for (var x = trimOffset; x < Size - trimOffset; x++)
//        for (var y = trimOffset; y < Size - trimOffset; y++)
//        for (var z = trimOffset; z < Size - trimOffset; z++)
//        {
//            var index = x + y * Size + z * Size * Size;
//
//            var culled = (x > 0 && x < Size - 1 && y > 0 && y < Size - 1 && z > 0 && z < Size - 1);
//            temp[index] = new VoxelRenderChunkData()
//            {
//                MeshId = 0,
//                MaterialId = 0,
//                ShouldCullFlag = culled
//            };
//        }
//
//        em.GetBuffer<VoxelRenderChunkElement>(entity).AddRange(temp);
//
//
////        //Iterate over all internal bits
////        for (var x = 1; x < Size - 1; x++)
////        for (var y = 1; x < Size - 1; y++)
////        for (var z = 1; x < Size - 1; z++)
////        {
////            var index = x + y * Size + z * Size * Size;
////            renderData.ShouldCullFlag[index] = true;
////        }
//    }
//
//
//    void GenerateUniverse(EntityManager em, EntityArchetype archetype)
//    {
//        for (var x = -UniverseSize; x <= UniverseSize; x++)
//        for (var y = -UniverseSize; y <= UniverseSize; y++)
//        for (var z = -UniverseSize; z <= UniverseSize; z++)
//        {
//            CreateChunk(new int3(x, y, z), em, archetype);
//        }
//    }
//
//    // Start is called before the first frame update
//    void Start()
//    {
//        GameManager.MasterRegistry.Mesh.Register("Fallback", _defaultMesh);
//        GameManager.MasterRegistry.Material.Register("Fallback", _defaultMaterial);
//
//
//        var world = World.Active; //        new World("Real World");
////        world.EntityManager.CompleteAllJobs();
//
//        var unloader = new SystemUnloader(world);
//
//
//
//        unloader.Unload<RenderMeshSystemV2>();
//        unloader.Unload<LodRequirementsUpdateSystem>();
//
//
//        var em = world.EntityManager;
//        var archetype = em.CreateArchetype(
//            typeof(VoxelRenderChunkElement),
//            typeof(VoxelChunkPosition));
//        GenerateUniverse(em, archetype);
//
//        disposable = world;
//    }
//
//    private void OnApplicationQuit()
//    {
//        disposable?.Dispose();
//    }
//}