using System.Collections;
using System.Collections.Generic;
using ECS.System;
using Unity.Entities;
using UnityEngine;

public class VoxelChanger : MonoBehaviour
{
    private Camera _camera;

    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            var dir = _camera.transform.forward;

            if (PhysicsUtil.RaycastEntity(ray.origin, ray.origin + ray.direction * 64, out var hit))
            {
                var active = World.Active;
                var manager = active.EntityManager;
                if (!manager.HasComponent(hit.entity, typeof(Disabled)))
                {
                    manager.AddComponent(hit.entity, typeof(Disabled));
                }
            }
        }
    }
}