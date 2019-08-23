using UnityEngine;

public class RenderGameObject
{
    public RenderGameObject(GameObject go, MeshRenderer mr = null, MeshFilter mf = null, MeshCollider mc = null)
    {
        GameObject = go;
        //TODO look into 'Bypassing lifetime check on unity GameObject' error
        Renderer = mr ? mr : go.GetComponent<MeshRenderer>();
        Filter = mf ? mf : go.GetComponent<MeshFilter>();
        Collider = mc ? mc : go.GetComponent<MeshCollider>();
    }

    public GameObject GameObject { get; }

    public Transform Transform => GameObject.transform;

    public MeshRenderer Renderer { get; }
    public MeshFilter Filter { get; }
    public MeshCollider Collider { get; }


    public RenderGameObject SetMesh(Mesh mesh)
    {
        Filter.mesh = Collider.sharedMesh = mesh;
        return this;
    }

    public RenderGameObject UpdateMesh()
    {
        return SetMesh(Filter.mesh);
    }

    public RenderGameObject SetMaterial(Material material)
    {
        Renderer.material = material;
        return this;
    }
}