using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PestelLib.PackageManager
{
    public class MessagePackSerializersUpdater
    {
        private const string PluginsCmdTemplate = "pestellib\\Tools\\MessagePack\\mpc\\mpc.exe -i \"{0}\" -o \"Assets\\MessagePackSerializers_Plugins.cs\" -r \"GeneratedResolverPlugins\"\n";
        private const string NormalCmdTemplate = "pestellib\\Tools\\MessagePack\\mpc\\mpc.exe -i \"{1}\" -o \"Assets\\MessagePackSerializers_Normal.cs\" -r \"GeneratedResolverNormal\"\n";
        private const string EndOfFile = "echo.&pause&goto:eof";
        private const string Template = PluginsCmdTemplate + NormalCmdTemplate + EndOfFile;


        [MenuItem("PestelCrew/MessagePack: Update serializers for IL2CPP")]
        public static void UpdateSerializers()
        {
            var assetsPath = Application.dataPath;
            var projectPath = Directory.GetParent(assetsPath);
            var projectPathFullName = projectPath.FullName.ToLowerInvariant();
            var projectPathName = projectPath.Name.ToLowerInvariant();
            var projectName = projectPathFullName + Path.DirectorySeparatorChar + projectPathName + ".Plugins.csproj";
#if UNITY_2018_3_OR_NEWER
            projectName = projectPathFullName + Path.DirectorySeparatorChar + "Assembly-CSharp-firstpass.csproj";
#endif
            if (File.Exists(projectName))
            {
                var mpcPath = projectPathFullName + "\\pestellib\\Tools\\MessagePack\\mpc\\mpc.exe";
                if (File.Exists(mpcPath))
                {
                    var cmdFileContent = string.Format(Template, projectPathName + ".Plugins.csproj", projectPathName + ".csproj");
#if UNITY_2018_3_OR_NEWER
                    cmdFileContent = string.Format(Template, "Assembly-CSharp-firstpass.csproj", "Assembly-CSharp.csproj");
#endif
                    FileInfo f = new FileInfo(projectPathFullName);
                    string drive = Path.GetPathRoot(f.FullName).Replace("\\", "");

                    cmdFileContent = drive + "\r\ncd " + projectPathFullName + "\r\n" + cmdFileContent;
                    var cmdFileName = projectPathFullName + "\\" + typeof(MessagePackSerializersUpdater).Name + ".cmd";
                    File.WriteAllText(cmdFileName, cmdFileContent);

                    ExecuteCmd(cmdFileName, "", projectPathFullName);

                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.LogError("Can't find mpc.exe utility");
                }
            }
            else
            {
                Debug.LogError("Can't find <ProjectName>.Plugins.csproj");
            }
        }

        private static void ExecuteCmd(string cmd, string arguments, string workingDirectory)
        {
            ProcessStartInfo info = new ProcessStartInfo(cmd); 
            info.UseShellExecute = true;
            info.Verb = "runas";
            info.WorkingDirectory = workingDirectory;
            info.Arguments = arguments;

            info.RedirectStandardOutput = false;
            info.RedirectStandardError = false;

            Process.Start(info);
        }
    }
}
