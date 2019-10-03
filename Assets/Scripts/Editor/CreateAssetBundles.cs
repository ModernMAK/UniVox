using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class CreateAssetBundles
    {
        [MenuItem("Assets/Build AssetBundles")]
        private static void BuildAllAssetBundles()
        {
            const BuildTarget target = BuildTarget.StandaloneWindows;
            var output = Path.Combine("Assets", "AssetBundles");
            var streaming = Path.Combine(Application.streamingAssetsPath);

            EnsureDirectory(output);
            var manifest = BuildPipeline.BuildAssetBundles(output, BuildAssetBundleOptions.None, target);


            Debug.Log($"Saved to {output} and {streaming} -> {manifest.GetAllAssetBundles().Length}");
            DirectoryCopy(output, streaming);
        }


        private static void EnsureDirectory(string directory)
        {
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName)
        {
            if (sourceDirName == destDirName)
                return;

            EnsureDirectory(destDirName);

            foreach (var folderPath in Directory.GetDirectories(sourceDirName, "*", SearchOption.AllDirectories))
                if (!Directory.Exists(folderPath.Replace(sourceDirName, destDirName)))
                    Directory.CreateDirectory(folderPath.Replace(sourceDirName, destDirName));

            foreach (var filePath in Directory.GetFiles(sourceDirName, "*.*", SearchOption.AllDirectories))
            {
                var fileDirName = Path.GetDirectoryName(filePath).Replace("\\", "/");
                var fileName = Path.GetFileName(filePath);
                var newFilePath = Path.Combine(fileDirName.Replace(sourceDirName, destDirName), fileName);

                newFilePath = Path.GetFullPath(newFilePath);
                var oldFilePath = Path.GetFullPath(filePath);
                if (oldFilePath == newFilePath)
                    continue;

                File.Copy(filePath, newFilePath, true);
            }
        }
    }
}