using Unity.Mathematics;
using UnityEngine;
using UniVox.Launcher;
using UniVox.Types;

namespace UniVox
{
    public class UnivoxStartup : MonoBehaviour
    {
        public Material defaultMat;
        public int3 offset = 0;

        public int3 wSize = 0;

        // Start is called before the first frame update
        private void Start()
        {
            if (!GameManager.Registry.Mods.IsRegistered(BaseGameMod.ModPath))
                GameManager.Registry.Mods.Register(BaseGameMod.ModPath);
            GameManager.Registry.ArrayMaterials.Register(new ArrayMaterialKey(BaseGameMod.ModPath, "Default"),
                defaultMat);

            var world = GameManager.Universe.GetOrCreate(0, "UniVox");
        }


        private void OnApplicationQuit()
        {
            GameManager.Universe.Dispose();
            GameManager.NativeRegistry.Dispose();
        }

        private void OnDestroy()
        {
            GameManager.Universe.Dispose();
            GameManager.NativeRegistry.Dispose();
        }
    }
}