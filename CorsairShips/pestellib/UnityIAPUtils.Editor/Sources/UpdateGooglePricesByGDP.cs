using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PestelLib.UnityIAPUtils.Editor.Sources
{
    public class UpdateGooglePricesByGDP
    {
        [MenuItem("PestelCrew/UnityIAP/Update prices in google csv based on country GDP")]
        public static void UpdatePrices()
        {
            var path = EditorUtility.OpenFilePanel("Select google .csv from google play developer console", "", "csv");
            if (string.IsNullOrEmpty(path)) return;

            var projectPath = Application.dataPath.Replace("Assets", string.Empty);
            var scriptPath = projectPath + "pestellib/Tools/PriceTools/GooglePlayCountryPriceChanger.py";

            if (!ExistsOnPath("python.exe"))
            {
                Debug.LogError("python.exe not found. You have to keep its path in system variable PATH.");
                return;
            }

            var scriptArguments = "/c\"python.exe " + " " + scriptPath + " " + path + "\"";
            Debug.Log("Running command: " + scriptArguments);
            ExecuteCommand(scriptArguments);

            var resultingPath = Path.GetDirectoryName(path) + "/ExportPrices.csv";
            Debug.Log("Output file: " + resultingPath);

            var psi = new ProcessStartInfo(resultingPath);
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        public static bool ExistsOnPath(string fileName)
        {
            return GetFullPath(fileName) != null;
        }

        public static string GetFullPath(string fileName)
        {
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            var values = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in values.Split(';'))
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
            return null;
        }

        public static void ExecuteCommand(string cmd)
        {
            var thread = new Thread(delegate () { Command(cmd); });
            thread.Start();
        }

        static void Command(string cmd)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", cmd);
            processInfo.CreateNoWindow = false;
            processInfo.UseShellExecute = false;

            var process = Process.Start(processInfo);

            process.WaitForExit();
            process.Close();
        }
    }
}