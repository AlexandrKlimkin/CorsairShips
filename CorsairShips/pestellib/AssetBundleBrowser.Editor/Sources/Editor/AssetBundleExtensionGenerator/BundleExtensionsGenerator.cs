using System.IO;
using UnityEditor;
using UnityEngine;

class BundleExtensionsGenerator
{
    public static readonly string FILE_EXTENSION = ".unity3d";

    [MenuItem("Assets/Generate file-extesnsions to the bundles")]
    static void AddExtensionsOnFiles()
    {
        var paths = new string[]
        {
            Application.dataPath.Replace("Assets", "") + "AssetBundles/StandaloneWindows/",
            Application.dataPath.Replace("Assets", "") + "AssetBundles/Android/"
        };

        for (int i = 0; i < paths.Length; i++)
        {
            var files = Directory.GetFiles(paths[i]);
            foreach (var filePath in files)
            {
                if (filePath.Contains("."))
                    continue;
                System.IO.File.Move(filePath, filePath + FILE_EXTENSION);
                Debug.Log(string.Format("File {0} moved!", filePath));
            }
        }
        

    }
}

