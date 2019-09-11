using UnityEngine;

public class RenderingPool
{
    private readonly Transform _container;

    public RenderingPool()
    {
        _container = new GameObject("Mesh Engine Container").transform;
        Meshes = new DelegatePool<Mesh>(() => new Mesh());
        GameObjects = new DelegatePool<RenderGameObject>(CreateRenderGameObject);
    }


    public DelegatePool<Mesh> Meshes { get; }

    public DelegatePool<RenderGameObject> GameObjects { get; }

    private RenderGameObject CreateRenderGameObject()
    {
        var go = new GameObject("Render Game Object", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        go.transform.parent = _container.transform;
        return new RenderGameObject(go);
    }
}