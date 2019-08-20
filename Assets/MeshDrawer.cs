using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshDrawer : MonoBehaviour
{
    [SerializeField] private RenderMesh RenderMesh;

    private Mesh Mesh => RenderMesh.mesh;
    private Material Material => RenderMesh.material;
    private int SubMesh => RenderMesh.subMesh;
    private int Layer => RenderMesh.layer;
    private ShadowCastingMode CastShadows => RenderMesh.castShadows;
    private bool RecieveShadows => RenderMesh.receiveShadows;

    private void Update()
    {
        Graphics.DrawMesh(Mesh, transform.localToWorldMatrix, Material, Layer, null, SubMesh, null, CastShadows,
            RecieveShadows);
    }
}