using UnityEditor;

namespace Editor
{
    public static class CreateAssetBundles
    {
        [MenuItem ("Assets/Build AssetBundles")]
        static void BuildAllAssetBundles ()
        {
            BuildPipeline.BuildAssetBundles ("Assets/AssetBundles", BuildAssetBundleOptions.None, BuildTarget.StandaloneOSXUniversal);
        }
    }
}