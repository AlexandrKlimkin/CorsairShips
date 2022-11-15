using System.IO;
using UnityEngine;
#if UNITY_EDITOR	
using UnityEditor;
#endif

namespace PestelLib.AssetBundles
{
    public class Utility
    {
        public static byte[] LoadBytes(string fileName)
        {
            if (File.Exists(Application.persistentDataPath + "/" + fileName))
            {
                using (FileStream file = File.Open(Application.persistentDataPath + "/" + fileName, FileMode.Open))
                {
                    byte[] data = new byte[file.Length];
                    file.Read(data, 0, (int)file.Length);
                    return data;
                }
            }
            return null;
        }

        public static bool Exists(string fileName)
        {
            return File.Exists(Application.persistentDataPath + "/" + fileName);
        }

        public static void SaveBytesToFile(byte[] data, string fileName)
        {

            int idx = fileName.IndexOf('/');
            if (idx != -1)
            {
                string dirPath = fileName.Substring(0, idx);
                Directory.CreateDirectory(Application.persistentDataPath + "/" + dirPath);
            }

            using (FileStream file = File.Create(Application.persistentDataPath + "/" + fileName))
            {
                file.Write(data, 0, data.Length);
            }
        }


        public const string AssetBundlesOutputPath = "AssetBundles";

        public static string GetPlatformName()
        {
#if UNITY_EDITOR
			return GetPlatformForAssetBundles(EditorUserBuildSettings.activeBuildTarget);
#else
            return GetPlatformForAssetBundles(Application.platform);
#endif
        }

#if UNITY_EDITOR
		private static string GetPlatformForAssetBundles(BuildTarget target)
		{
			switch(target)
			{
			case BuildTarget.Android:
				return "Android";
			case BuildTarget.iOS:
				return "iOS";
			case BuildTarget.WebGL:
				return "WebGL";
			case BuildTarget.WebPlayer:
				return "WebPlayer";
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
				return "Windows";
			case BuildTarget.StandaloneOSXIntel:
			case BuildTarget.StandaloneOSXIntel64:
			case BuildTarget.StandaloneOSXUniversal:
				return "OSX";
				// Add more build targets for your own.
				// If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
			default:
				return null;
			}
		}
#endif

        private static string GetPlatformForAssetBundles(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.WebGLPlayer:
                    return "WebGL";
                case RuntimePlatform.OSXWebPlayer:
                case RuntimePlatform.WindowsWebPlayer:
                    return "WebPlayer";
                case RuntimePlatform.WindowsPlayer:
                    return "Windows";
                case RuntimePlatform.OSXPlayer:
                    return "OSX";
                // Add more build targets for your own.
                // If you add more targets, don't forget to add the same platforms to GetPlatformForAssetBundles(RuntimePlatform) function.
                default:
                    return null;
            }
        }
    }
}