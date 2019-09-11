using System.IO;
using Rendering;
using UnityEditor;
using UnityEngine;

public class BaseGameMod : AbstractMod
{
    public const string ModPath = "BaseGame";

    public override void Initialize(ModInitializer initializer)
    {
        var meshReg = initializer.Registries.Mesh;

        var temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var mr = temp.GetComponent<MeshRenderer>();
        using (var asset = ModAssets.LoadMaterialBundle(Application.dataPath, ModPath))
        {
            var tempMat = asset.LoadAsset<Material>("ErrorMaterial");
            mr.material = new Material(tempMat);
        }
    }

}