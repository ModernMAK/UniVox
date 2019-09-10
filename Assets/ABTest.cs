using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var mr = temp.GetComponent<MeshRenderer>();
//        var asset = ModResources.Load<Material>("Error",Path.Combine(Application.streamingAssetsPath, "Materials"));
        using (var asset = ModAssets.LoadMaterialBundle(Application.streamingAssetsPath, "basegame"))
        {
            var tempMat = asset.LoadAsset<Material>("ErrorMaterial");
            mr.material = new Material(tempMat);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}