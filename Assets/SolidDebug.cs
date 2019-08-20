using System.Collections;
using Types;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class SolidDebug : MonoBehaviour
{
    private NativeArray<bool> _active;

    // Start is called before the first frame update
    void Start()
    {
        var temp = GetComponent<WorldBehaviour>();
        StartCoroutine(WaitForLoad(temp));
    }

    private IEnumerator WaitForLoad(WorldBehaviour wb)
    {
        var key = int3.zero;
        while (!wb.World.ContainsKey(key))
            yield return null;
        _active = wb.World[key].ActiveFlags;
    }

    private void OnDrawGizmos()
    {
        if (!_active.IsCreated || !enabled)
            return;
        var half = new float3(1f / 2f);
        for (var i = 0; i < Chunk.FlatSize; i++)
        {
            var v = new VoxelPos8(i);
            if (_active[i])
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;
            Gizmos.DrawCube(v.Position + half, half);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}