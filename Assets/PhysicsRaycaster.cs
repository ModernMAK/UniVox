using System;
using System.Collections;
using System.Collections.Generic;
using Types;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using UnityEngine;
using Unity.Physics;
using UnityEdits;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Ray = Unity.Physics.Ray;

public class PhysicsRaycaster : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    private Camera _camera;
    private RaycastInput? _lastRay;
    private int3? _lastVoxel;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            var physicsSystem = World.Active.GetOrCreateSystem<BuildPhysicsWorld>();
            var collisionWorld = physicsSystem.PhysicsWorld.CollisionWorld;

            const float distance = ChunkSize.AxisSize * 8; //Raycast at least 8 chunks away
            var camRay = _camera.ScreenPointToRay(Input.mousePosition);
            var start = camRay.origin;
            var direction = camRay.direction;

            RaycastInput input = new RaycastInput()
            {
                Start = start,
                End = start + direction * distance,
                Filter = CollisionFilter.Default
            };
            _lastRay = input;
            if (collisionWorld.CastRay(input, out var closestHit))
            {
                //one decimal is pretty good for our use cases right now
                const int scale = 1;
                var posRounded = math.floor(closestHit.Position * scale) / scale;
                var normRounded = math.floor(closestHit.SurfaceNormal * scale) / scale;

                var voxSpace = UnivoxPhysics.ToVoxelSpace(closestHit.Position, closestHit.SurfaceNormal);

                _lastVoxel = voxSpace;
//                Debug.Log($"UnitySpace: Hit at {posRounded}, facing {normRounded}.");
//                Debug.Log($"VoxelSpace: Hit at {voxSpace}, facing {normRounded}.");
                Debug.Log($"UnitySpace: Hit at {posRounded}, facing {normRounded}.\nVoxelSpace: Hit at {voxSpace}, facing {normRounded}.");
            }
            else
            {
                Debug.Log($"Missed");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (_lastRay == null || _lastVoxel == null)
            return;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(_lastRay.Value.Start, _lastRay.Value.End);
        Gizmos.DrawWireCube(UnivoxPhysics.ToUnitySpace(_lastVoxel.Value),new Vector3(1f,1f,1f));
    }
}