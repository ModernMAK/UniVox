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

public class WallE : MonoBehaviour
{
    public int Size;

    public GameObject Prefab;

    private World disposable;

    // Start is called before the first frame update
    void Start()
    {
        var world = World.Active; //        new World("Real World");
        world.DestroySystem(world.GetExistingSystem<RenderMeshSystemV2>());
        world.DestroySystem(world.GetExistingSystem<LodRequirementsUpdateSystem>());
//        ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world);
        var em = world.EntityManager;
        var flatSize = Size * Size * Size;
        var prefab =
            GameObjectConversionUtility.ConvertGameObjectHierarchy(Prefab,
                new GameObjectConversionSettings(world, default));
        using (var array = new NativeArray<Entity>(flatSize, Allocator.TempJob))
        {
            em.Instantiate(prefab, array);
            em.AddComponent<Static>(array);
            for (var x = 0; x < Size; x++)
            for (var y = 0; y < Size; y++)
            for (var z = 0; z < Size; z++)
            {
                var i = x + y * Size + z * Size * Size;
                em.SetComponentData(array[i], new Translation() {Value = new float3(x, y, z)});

                if (x != 0 && y != 0 && z != 0 && x != Size - 1 && y != Size - 1 && z != Size - 1)
                {
                    em.AddComponent<DontRenderTag>(array[i]);
                }
            }
        }
        em.DestroyEntity(prefab);
        world.GetOrCreateSystem<RenderMeshSystemV3>();
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