using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBoundVisualizer : MonoBehaviour
{
    [SerializeField] private bool _hideCollider;
    [SerializeField] private Color _colliderBoundColor;
    [SerializeField] private Color _colliderBoundErrorColor;
    [SerializeField] private bool _hideMesh;
    [SerializeField] private Color _meshBoundColor;
    [SerializeField] private Color _meshBoundErrorColor;


    private MeshRenderer _mesh;
    private MeshCollider _collider;

    private void Awake()
    {
        _mesh = GetComponent<MeshRenderer>();
        _collider = GetComponent<MeshCollider>();
    }

    private void OnDrawGizmos()
    {
        if (!_hideMesh)
        {
            if (_mesh)
            {
                Gizmos.color = _meshBoundColor;
                var bounds = _mesh.bounds;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
            else
            {
                Gizmos.color = _meshBoundErrorColor;
                var bounds = new Bounds(Vector3.zero, Vector3.one);
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }

        if (!_hideCollider)
        {
            if(_collider != null)
            {
                Gizmos.color = _colliderBoundColor;
                var bounds = _collider.bounds;
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
            else
            {
                Gizmos.color = _colliderBoundErrorColor;
                var bounds = new Bounds(Vector3.zero, Vector3.one);
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }
}