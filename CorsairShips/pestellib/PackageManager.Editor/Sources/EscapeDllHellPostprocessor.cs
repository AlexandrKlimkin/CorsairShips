using System.Diagnostics;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace PestelLib.PackageManager
{
    public class EscapeDllHellPostprocessor : AssetPostprocessor
    {
        private const string GetEscapeDllHellVersionCmd = "git log -n1 --pretty=format:\"%h\" " + EscapeDllHellGenerator.EscapeDllHellBatFile;

        static public void ShowEscapeDllHellVersion()
        {
            ExecuteCommand(GetEscapeDllHellVersionCmd, Path.GetFullPath("./"));
        }

        public static string ExecuteCommand(string command, string workingDir)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.WorkingDirectory = workingDir;
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();
            exitCode = process.ExitCode;

            if (exitCode != 0)
            {
                var message = GetEscapeDllHellVersionCmd + " returned error code: " + process.ExitCode +
                              "\nplease run it from command line and see what happened";
                Debug.LogError(message);
                EditorUtility.DisplayDialog("Error", message, "OK");
            }
            process.Close();
            return output;
        }

        static public string EscapeDllHellFullPath 
        {
            get { return Path.GetFullPath("./") + EscapeDllHellGenerator.EscapeDllHellBatFile; }
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths) 
        {
            #if UNITY_EDITOR_WINDOWS
            if (!File.Exists(EscapeDllHellFullPath)) return;
            var oldVersion = EditorPrefs.GetString(EscapeDllHellFullPath);
            var newVersion = ExecuteCommand(GetEscapeDllHellVersionCmd, Path.GetFullPath("./"));

            Debug.Log("escape dll hell version was " + oldVersion + " and now it " + newVersion);

            if (oldVersion != newVersion)
            {
                EscapeDllHellGenerator.ExecuteEscapeDllHell();
            }
            EditorPrefs.SetString(EscapeDllHellFullPath, newVersion);
            #endif
        }
    }
}