using DefaultNamespace;
using Unity.Mathematics;
using UnityEngine;

public class RenderTester : MonoBehaviour
{
    public void Start()
    {
        var mf = GetComponent<MeshFilter>();
        var m = new Mesh();
        var chunk = new Chunk();

        var handle = RenderUtilV2.VisiblityPass(chunk);
        RenderUtilV2.Render(chunk, m, handle);

        chunk.Dispose();

        m.RecalculateBounds();
        m.RecalculateNormals();    
        m.RecalculateTangents();
        mf.mesh = m;
    }
}