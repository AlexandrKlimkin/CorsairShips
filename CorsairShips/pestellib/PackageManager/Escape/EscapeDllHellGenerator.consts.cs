using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PackageManager.Escape
{
    public partial class EscapeDllHellGenerator
    {
        public const string EscapeDllHellUserPrefsFile = "escape_dll_hell.selected";
        // win
        public const string EscapeDllHellBatFile = "escape_dll_hell.bat";
        private const string CmdFileBodyStartKey = "rem BODY_START";

        private const string PackageManagerWin = "\r\npestellib\\Tools\\PackageManager\\win\\PackageManager.exe --UpdateDependencies\r\n";

        private const string CmdRegularLibDeleteTemplate =
            "if exist \"Assets\\Plugins\\LibsSymlinks\\{0}\\\" (\r\n" +
            "rmdir \"Assets\\Plugins\\LibsSymlinks\\{0}\" /s /q\r\n" +
            ")\r\n" +
            "if exist \"Assets\\Plugins\\LibsSymlinks\\{0}\" (\r\n" +
            "del \"Assets\\Plugins\\LibsSymlinks\\{0}\" /f /q\r\n" +
            ")\r\n";

        private const string CmdRegularLibTemplate = CmdRegularLibDeleteTemplate +
                                                     "mklink /d \"Assets\\Plugins\\LibsSymlinks\\{0}\" \"..\\..\\..\\{1}Sources\"";

        private const string CmdEditorLibDeleteTemplate =
            "if exist \"Assets\\Plugins\\LibsSymlinks\\Editor\\{0}\\\" (\r\n" +
            "rmdir \"Assets\\Plugins\\LibsSymlinks\\Editor\\{0}\" /s /q\r\n" +
            ")\r\n" +
            "if exist \"Assets\\Plugins\\LibsSymlinks\\Editor\\{0}\" (\r\n" +
            "del \"Assets\\Plugins\\LibsSymlinks\\Editor\\{0}\" /f /q\r\n" +
            ")\r\n";

        private const string CmdEditorLibTemplate = CmdEditorLibDeleteTemplate +
                                                    "mklink /d \"Assets\\Plugins\\LibsSymlinks\\Editor\\{0}\" \"..\\..\\..\\..\\{1}Sources\"\r\n";

        private const string CmdSharedLogicLibDeleteTemplate =
            "if exist \"ProjectLib\\ConcreteSharedLogic\\Sources\\Modules\\{0}\\\" (\r\n" +
            "rmdir \"ProjectLib\\ConcreteSharedLogic\\Sources\\Modules\\{0}\" /s /q\r\n" +
            ")\r\n" +
            "if exist \"ProjectLib\\ConcreteSharedLogic\\Sources\\Modules\\{0}\" (\r\n" +
            "del \"ProjectLib\\ConcreteSharedLogic\\Sources\\Modules\\{0}\" / f /q\r\n" +
            ")\r\n";

        private const string CmdSharedLogicLibTemplate = CmdSharedLogicLibDeleteTemplate +
                                                         "mklink /d \"ProjectLib\\ConcreteSharedLogic\\Sources\\Modules\\{0}\" \"..\\..\\..\\..\\{1}SourcesSL\"\r\n";

        // mac
        private const string EscapeDllHellShellFile = "escape_dll_hell.sh";
        private const string ShellFileBodyStartKey = "# BODY_START";
        private const string ShellFileBodyEndKey = "# BODY_END";
        private const string ShellRegularLibTemplate = "ln -s \"../../../{1}Sources\" \"./Assets/Plugins/LibsSymlinks/{0}\"";
        private const string ShellEditorLibTemplate = "ln -s \"../../../../{1}Sources\" \"./Assets/Plugins/LibsSymlinks/Editor/{0}\"";
        private const string ShellSharedLogicLibTemplate = "rm \"./ProjectLib/ConcreteSharedLogic/Sources/Modules/{0}\"\nln -s \"../../../../{1}SourcesSL\" \"./ProjectLib/ConcreteSharedLogic/Sources/Modules/{0}\"\n";


        private Dictionary<string, string> TemplateProjectsMap = new Dictionary<string, string>()
        {
            { "SharedLogicTemplate","ConcreteSharedLogic"}
        };

        private string CmdFilePath
        {
            get
            {
                return Path.GetFullPath(_basePath) + EscapeDllHellBatFile;
            }
        }

        private string EscapeDllUserPrefsPath
        {
            get { return Path.GetFullPath(_basePath) + EscapeDllHellUserPrefsFile; }
        }

        private string ShellFilePath
        {
            get
            {
                return Path.GetFullPath(_basePath) + EscapeDllHellShellFile;
            }
        }

        const string CmdFileTemplate = @"
@echo off

pushd ""%~dp0""
echo %cd%

rem rd /s /q ""Assets\Plugins\LibsSymlinks""
mkdir ""Assets\Plugins\LibsSymlinks""
mkdir ""Assets\Plugins\LibsSymlinks\Editor""

rem BODY_START

rem BODY_END

python PestelLib\Tools\BuildTools\pestellibgen.py --project-root=%~dp0 --sln-name=PestelProjectLib.sln

rem echo.&pause&goto:eof

popd";


        const string ShellFileTemplate = "#!/bin/bash\nrm -r \"./Assets/Plugins/LibsSymlinks\"\nmkdir \"./Assets/Plugins/LibsSymlinks\"\nmkdir \"./Assets/Plugins/LibsSymlinks/Editor\"\n# BODY_START\n # BODY_END\n";
        public const string DllHellGeneratorProjectName = "PackageManager.Editor";
    }
}
