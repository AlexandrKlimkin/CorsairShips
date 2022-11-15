using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PestelLib.PackageManager
{
    public class EscapeDllHellGenerator
    {
        private Vector2 _scrollPosition;
        // win
        public const string EscapeDllHellBatFile = "escape_dll_hell.bat";
        // mac
        private const string EscapeDllHellShellFile = "escape_dll_hell.sh";

        private Dictionary<string, string> TemplateProjectsMap = new Dictionary<string, string>()
        {
            { "SharedLogicTemplate","ConcreteSharedLogic"}
        };

        [MenuItem("PestelCrew/Package Manager/Open package manager")]
        static void PackageManager()
        {
#if UNITY_EDITOR_OSX
            ProcessStartInfo info = new ProcessStartInfo("dotnet " + Path.GetFullPath("./") + "pestellib/Tools/PackageManager/nix/PackageManager.dll ");
            info.UseShellExecute = true;
            info.Verb = "runas";
            Process.Start(info);
#elif UNITY_EDITOR_WIN
            ProcessStartInfo info = new ProcessStartInfo(Path.GetFullPath("./") + "pestellib/Tools/PackageManager/win/PackageManager.exe");
            info.UseShellExecute = true;
            info.Verb = "runas";
            Process.Start(info);
#endif
        }

        [MenuItem("PestelCrew/Package Manager/Re-create symbolic links")]
        static public void ExecuteEscapeDllHell()
        {
#if UNITY_EDITOR_OSX
            ProcessStartInfo info = new ProcessStartInfo(Path.GetFullPath("./") + EscapeDllHellGenerator.EscapeDllHellShellFile);
            info.UseShellExecute = true;
            info.Verb = "runas";
            Process.Start(info);
#elif UNITY_EDITOR_WIN
            ProcessStartInfo info = new ProcessStartInfo(Path.GetFullPath("./") + EscapeDllHellGenerator.EscapeDllHellBatFile);
            info.UseShellExecute = true;
            info.Verb = "runas";
            Process.Start(info);
#endif
        }
    }
}