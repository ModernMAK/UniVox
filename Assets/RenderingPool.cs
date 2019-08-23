using UnityEngine;

public class RenderingPool
{
    private readonly Transform _container;

    public RenderingPool()
    {
        _container = new GameObject("Mesh Engine Container").transform;
        Meshes = new Pool<Mesh>(() => new Mesh());
        GameObjects = new Pool<RenderGameObject>(CreateRenderGameObject);
    }


    public Pool<Mesh> Meshes { get; }

    public Pool<RenderGameObject> GameObjects { get; }

    private RenderGameObject CreateRenderGameObject()
    {
        var go = new GameObject("Render Game Object", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        go.transform.parent = _container.transform;
        return new RenderGameObject(go);
    }
}