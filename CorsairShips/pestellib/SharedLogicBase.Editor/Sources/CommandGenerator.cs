using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace PestelLib.SharedLogicBase.Editor
{
    public class Method
    {
        public string moduleName;
        public string returnType;
        public string methodName;
        public List<List<string>> arguments;
    }

    class CommandGenerator : AssetPostprocessor
    {
        private const string UpdatecommandhelperoutputTxt = "UpdateCommandHelperOutput.txt";
        public static string[] modulesToProcess;

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            modulesToProcess = importedAssets;

            foreach (string str in importedAssets)
            {
                if (str.Contains("ConcreteSharedLogic") && str.Contains("Modules") && Path.GetExtension(str).ToLower() == ".cs")
                {
                    Debug.Log("Module \"" + str + " has changed. Updating CommandHelper.");
                    UpdateCommandHelper();
                    break;
                }
            }
        }

        [MenuItem("PestelCrew/SharedLogic/UpdateCommandHelper")]
        public static void UpdateCommandHelper()
        {
            var pwd = Path.GetFullPath("./");
            var args = string.Format(
                "--projDir {0} --protocolFile \"{1}\" --wrapperFile \"{2}\" --sharedLogicCoreFile \"{3}\" --autoCommandsNamespace \"{4}\"",
                pwd,
                CommandGeneratorSettings.Instance.ProtocolFilePath,
                CommandGeneratorSettings.Instance.WrapperFilePath,
                CommandGeneratorSettings.Instance.SharedLogicCorePath,
                CommandGeneratorSettings.Instance.AutoCommandsNamespace);

            var app = string.Format("{0}PestelLib{1}Tools{1}BuildTools{1}UpdateCommandHelper.exe", pwd, Path.DirectorySeparatorChar);
            Debug.Log("Starting " + app + " " + args);
            var processInfo = new ProcessStartInfo(app, args);
            var process = new Process();
            process.StartInfo = processInfo;
            //process.StartInfo.UseShellExecute = false;
            //process.StartInfo.RedirectStandardOutput = true;
            //process.StartInfo.RedirectStandardError = true;
            
            if (!process.Start())
            {
                Debug.LogError("Can't start app");
                return;
            }

            process.WaitForExit();
            if (process.ExitCode == 0)
            {
                if (File.Exists(UpdatecommandhelperoutputTxt))
                {
                    var output = File.ReadAllText(UpdatecommandhelperoutputTxt);
                    Debug.Log(output);
                    File.Delete(UpdatecommandhelperoutputTxt);
                }
                AssetDatabase.Refresh();
            }
            else
            {
                if (File.Exists(UpdatecommandhelperoutputTxt))
                {
                    var error = File.ReadAllText(UpdatecommandhelperoutputTxt);
                    Debug.LogError(error);
                    File.Delete(UpdatecommandhelperoutputTxt);
                }
            }
        }
    }
}