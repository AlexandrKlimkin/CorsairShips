using System.IO;
using UnityEditor;
using UnityEngine;

namespace PestelLib.UtilsEditor
{
    public class GitUtils
    {
        [MenuItem("PestelCrew/Git/Fix local filenames case")]
        public static void FixLocalFilenamesCase()
        {
            var repositoryPath = Application.dataPath.Replace("/Assets", "/");
            var toolPath = repositoryPath + "pestellib/Tools/git-unite/Git.Unite.exe";

            if (System.IO.File.Exists(toolPath))
            {
                System.Diagnostics.Process.Start(toolPath, "-o " + repositoryPath);
            }
            else
            {
                UnityEngine.Debug.LogError("Can't find tool: " + toolPath);
            }
        }

        [MenuItem("PestelCrew/Git/Stage local filename changes")]
        public static void AddFilenamesChangeToStage()
        {
            var repositoryPath = Application.dataPath.Replace("/Assets", "/");
            var toolPath = repositoryPath + "pestellib/Tools/git-unite/Git.Unite.exe";

            if (System.IO.File.Exists(toolPath))
            {
                System.Diagnostics.Process.Start(toolPath, repositoryPath);
            }
            else
            {
                UnityEngine.Debug.LogError("Can't find tool: " + toolPath);
            }
        }
    }
}