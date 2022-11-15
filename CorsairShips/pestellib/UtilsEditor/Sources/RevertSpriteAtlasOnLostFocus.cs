using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PestelLib.UtilsEditor { 
    [InitializeOnLoad]
    public class RevertSpriteAtlasOnLostFocus
    {
        static RevertSpriteAtlasOnLostFocus()
        { 
            EditorWindowFocusUtility.OnUnityEditorFocus += EditorWindowFocusUtilityOnOnUnityEditorFocus;
        }

        private static void EditorWindowFocusUtilityOnOnUnityEditorFocus(bool focus)
        {
            if (focus) return;

            if (!EditorPrefs.GetBool(SpriteAtlasAutoRevertPrefsKey, false)) return;

            if (!Directory.Exists(Application.dataPath + "/../.git"))
            {
                Debug.Log("Can't find .git folder " + Application.dataPath + "/../.git");
                return;
            }

            CleanUpAtlases();
            Debug.Log("Spriteatlases was reverted");
        }

        private const string gitCleanUpCmd = "git checkout *.spriteatlas";
        private const string SpriteAtlasAutoRevertPrefsKey = "SpriteAtlasAutoRevert";

        static public void CleanUpAtlases()
        {
            ExecuteCommand(gitCleanUpCmd, Application.dataPath);
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
                var message = gitCleanUpCmd + " returned error code: " + process.ExitCode +
                              "\nplease run it from command line and see what happened";
                Debug.LogError(message);
                EditorUtility.DisplayDialog("Error", message, "OK");
            }
            process.Close();
            return output;
        }

        [MenuItem("PestelCrew/Spriteatlas auto-revert/Enable spriteatlas editing")]
        public static void EnableSpriteatlasEditing()
        {
            EditorPrefs.SetBool(SpriteAtlasAutoRevertPrefsKey, false);
        }

        [MenuItem("PestelCrew/Spriteatlas auto-revert/Disable spriteatlas editing")]
        public static void DisableSpriteatlasEditing()
        {
            EditorPrefs.SetBool(SpriteAtlasAutoRevertPrefsKey, true);
        }

        [MenuItem("PestelCrew/Spriteatlas auto-revert/Enable spriteatlas editing", true)]
        public static bool EnableSpriteatlasEditingActive()
        {
            return EditorPrefs.GetBool(SpriteAtlasAutoRevertPrefsKey, false);
        }

        [MenuItem("PestelCrew/Spriteatlas auto-revert/Disable spriteatlas editing", true)]
        public static bool DisableSpriteatlasEditingActive()
        {
            return !EditorPrefs.GetBool(SpriteAtlasAutoRevertPrefsKey, false);
        }
    }
}