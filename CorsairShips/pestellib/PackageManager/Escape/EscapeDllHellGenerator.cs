using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace PackageManager.Escape
{
    class EnabledByUser
    {
        public string[] Items = new string[]{};
        public string[] Exclude = new string[]{};
    }

    public partial class EscapeDllHellGenerator
    {
        private readonly string _basePath;
        private readonly List<ModuleState> _modules = new List<ModuleState>();
        private readonly HashSet<string> _excludedModules = new HashSet<string>();

        public ModuleState[] ModuleStates => _modules.ToArray();
        public bool IsModuleExcluded(string module) => _excludedModules.Contains(module);
        public EscapeDllHellGenerator(string basePath)
        {
            _basePath = basePath;

            Init();
        }

        public void AddDependencies(ModuleState module, int depth)
        {
            if(_excludedModules.Contains(module.Name))
                return;
            module.Enabled = true;
            foreach (var dependency in module.Dependencies)
            {
                var dependencyModule = _modules.FirstOrDefault(x => x.Name == dependency);
                if (dependencyModule != null && !_excludedModules.Contains(dependencyModule.Name))
                {
                    dependencyModule.Enabled = true;
                    if (depth < 50)
                    {
                        depth++;
                        AddDependencies(dependencyModule, depth);
                    }
                    else
                    {
                        Logger.LogError("Probably you have some cycle references?");
                    }
                }
            }
        }

        public void RemoveModule(ModuleState module, HashSet<string> chain = null)
        {
            if (module == null)
                return;
            chain = chain ?? new HashSet<string>();
            if (module.UserEnabled || !module.Enabled)
                return;

            chain.Add(module.Name);
            foreach (var dep in module.Dependencies)
            {
                if (_modules.Any(m => !chain.Contains(m.Name) && m.Enabled && DependsOn(m, dep)))
                    continue;

                RemoveModule(_modules.FirstOrDefault(m => m.Name == dep), chain);
            }
            module.Enabled = _modules.Any(m => m.Enabled && !chain.Contains(m.Name) && DependsOn(m, module.Name));
        }

        public void WriteBatch()
        {
            WriteBatFile();
            WriteUserPrefsFile();
            WriteShellFile();
        }

        public void RunBatch()
        {
            WriteBatFile();
            WriteUserPrefsFile();
            WriteShellFile();
            ExecuteEscapeDllHell();
        }

        private void Init()
        {
            _modules.Clear();

            var path = Path.GetFullPath($"{_basePath}ProjectLib/");
            BuildProjectsList(path, "ProjectLib");

            path = Path.GetFullPath($"{_basePath}PestelLib/");
            BuildProjectsList(path, "PestelLib");

            ProcessDependencies();

            var dllHellGeneratorModule = _modules.FirstOrDefault(x => x.Name == DllHellGeneratorProjectName);
            if (dllHellGeneratorModule != null)
            {
                dllHellGeneratorModule.Enabled = true;
            }
        }

        private void BuildProjectsList(string baseDirectory, string libraryType)
        {
            if (!Directory.Exists(baseDirectory)) return;

            var directories = Directory.GetDirectories(baseDirectory);
            directories = directories.Select(x => x.Replace(baseDirectory, "")).ToArray();

            _modules.AddRange(
                directories
                    .Where(x => Directory.Exists(Path.Combine(baseDirectory + x, "Sources"))
                                || Directory.Exists(Path.Combine(baseDirectory + x, "SourcesSL")))
                    .Select(x => new ModuleState
                    {
                        FolderName = x,
                        Name = x,
                        Directory = baseDirectory + x + Path.DirectorySeparatorChar,
                        LibraryType = libraryType,
                        SharedLogicModule = Directory.Exists(Path.Combine(baseDirectory + x, "SourcesSL"))
                    })
            );
        }

        private EnabledByUser LoadUserPrefs()
        {
            if (File.Exists(EscapeDllUserPrefsPath))
            {
                var data = File.ReadAllText(EscapeDllUserPrefsPath);
                var d = JsonConvert.DeserializeObject<EnabledByUser>(data);
                return d;
            }
            return new EnabledByUser();
        }

        private void ProcessDependencies()
        {
            if (!File.Exists(CmdFilePath))
            {
                File.WriteAllText(CmdFilePath, CmdFileTemplate);
            }

            var userPrefs = LoadUserPrefs();
            var enabledByUser = userPrefs.Items;
            if (userPrefs.Exclude != null)
            {
                foreach (var s in userPrefs.Exclude)
                {
                    _excludedModules.Add(s);
                }
            }

            var escapeDllHellFile = File.ReadAllText(CmdFilePath);

            for (var i = 0; i < _modules.Count; i++)
            {
                //var projectDir = baseDirectory + _modules[i].Name + Path.DirectorySeparatorChar;
                var projectDir = _modules[i].Directory;

                var projects = Directory.GetFileSystemEntries(projectDir, "*.csproj");
                if (projects.Length > 1)
                {
                    Logger.LogWarn($"Folder {projectDir} contains multiple project files. Can't get dependencies from project");
                    _modules[i].Name += " [Project file not found]";
                    continue;
                }
                if (projects.Length == 0)
                {
                    Logger.LogWarn($"Folder {projectDir} doesn't contain any project files. Can't get dependencies from project");
                    _modules[i].Name += " [Project file not found]";
                    continue;
                }

                _modules[i].Name = Path.GetFileNameWithoutExtension(projects[0]);
                _modules[i].Dependencies = ExtractDependencies(projects[0]);
                var deps = _modules[i].Dependencies;
                for (var j = 0; j < deps.Count; ++j)
                {
                    var d = deps[j];
                    string templateMap;
                    if (TemplateProjectsMap.TryGetValue(d, out templateMap))
                        deps[j] = templateMap;
                }

                if (enabledByUser.Length != 0)
                    _modules[i].UserEnabled = enabledByUser.Contains(_modules[i].Name);
                else
                    _modules[i].UserEnabled = escapeDllHellFile.Contains(Path.DirectorySeparatorChar + _modules[i].FolderName + Path.DirectorySeparatorChar);

            }

            foreach (var moduleState in _modules.Where(m => m.UserEnabled))
            {
                if(_excludedModules.Contains(moduleState.Name))
                    RemoveModule(moduleState);
                else
                    AddDependencies(moduleState, 0);
            }
        }

        private static List<string> ExtractDependencies(string projectFilePath)
        {
            var result = new List<string>();
            if (File.Exists(projectFilePath))
            {
                var document = new XmlDocument();
                document.Load(projectFilePath);

                var namespaceManager = new XmlNamespaceManager(document.NameTable);
                namespaceManager.AddNamespace("msb", "http://schemas.microsoft.com/developer/msbuild/2003");

                // Select a list of nodes
                XmlNodeList nodes = document.SelectNodes("/msb:Project/msb:ItemGroup", namespaceManager);

                foreach (var node in nodes)
                {
                    var element = node as XmlElement;
                    if (element != null)
                    {
                        var references = element.SelectNodes("msb:ProjectReference", namespaceManager);
                        foreach (var reference in references)
                        {
                            var refNode = reference as XmlElement;
                            if (refNode != null)
                            {
                                var nameNodes = refNode.SelectNodes("msb:Name", namespaceManager);
                                if (nameNodes != null)
                                {
                                    result.Add(nameNodes[0].InnerText);
                                }
                            }
                        }
                    }
                }

                // netstandard projects support
                nodes = document.SelectNodes("/Project/ItemGroup");
                foreach (XmlNode node in nodes)
                {
                    var references = node.SelectNodes("ProjectReference");
                    foreach (XmlNode reference in references)
                    {
                        if (reference.Attributes == null)
                            continue;

                        var nameNodes = Path.GetFileNameWithoutExtension(reference.Attributes["Include"].Value);
                        if (nameNodes != null)
                        {
                            result.Add(nameNodes);
                        }
                    }

                    references = node.SelectNodes("PackageReference");
                    foreach (XmlNode reference in references)
                    {
                        if (reference.Attributes == null)
                            continue;

                        var nameNodes = reference.Attributes["Include"].Value;
                        if (nameNodes != null)
                        {
                            result.Add(nameNodes);
                        }
                    }
                }
            }

            return result;
        }

        ModuleState FindModule(string moduleName)
        {
            return _modules.FirstOrDefault(m => m.Name == moduleName);
        }

        bool DependsOn(string module, string dep)
        {
            var m = FindModule(module);
            if (m == null)
                return false;
            return DependsOn(m, dep);
        }

        bool DependsOn(ModuleState module, string dep)
        {
            return module.Dependencies.Any(d => d == dep || DependsOn(d, dep));
        }

        private void WriteUserPrefsFile()
        {
            var data =
                JsonConvert.SerializeObject(new EnabledByUser()
                {
                    Items = _modules.Where(m => m.UserEnabled && !_excludedModules.Contains(m.Name)).Select(m => m.Name).OrderBy(x => x.Contains(".Editor")).ThenBy(x => x).ToArray(),
                    Exclude = _excludedModules.ToArray()
                }, Formatting.Indented
            );

            File.WriteAllText(_basePath + EscapeDllHellUserPrefsFile, data);
        }

        private void WriteBatFile()
        {
            var escapeDllHellFile = CmdFileTemplate;
            if (File.Exists(CmdFilePath))
                escapeDllHellFile = File.ReadAllText(CmdFilePath);
            var startIndex = escapeDllHellFile.IndexOf(CmdFileBodyStartKey, StringComparison.InvariantCulture) +
                             CmdFileBodyStartKey.Length;
            var endIndex = escapeDllHellFile.IndexOf("rem BODY_END", StringComparison.InvariantCulture);

            var fileBeginText = escapeDllHellFile.Substring(0, startIndex);
            var fileEndText = escapeDllHellFile.Substring(endIndex, escapeDllHellFile.Length - endIndex);

            StringBuilder body = new StringBuilder();

            body.Append(PackageManagerWin);

            foreach (var moduleState in _modules)
            {
                if (moduleState.Enabled)
                {
                    var relativePath = moduleState.Directory.Replace(Path.GetFullPath(_basePath), "");

                    if (moduleState.FolderName.EndsWith("Editor"))
                    {
                        body.AppendLine(string.Format(CmdEditorLibTemplate, moduleState.FolderName, relativePath));
                    }
                    else
                    {
                        if (Directory.Exists(moduleState.Directory + "Sources"))
                        {
                            body.AppendLine(string.Format(CmdRegularLibTemplate, moduleState.FolderName, relativePath));
                        }

                        if (moduleState.SharedLogicModule)
                        {
                            body.AppendLine(string.Format(
                                CmdSharedLogicLibTemplate,
                                moduleState.FolderName,
                                relativePath)
                            );
                        }
                    }
                }
                else
                {
                    if (moduleState.FolderName.EndsWith("Editor"))
                    {
                        body.AppendLine(string.Format(CmdEditorLibDeleteTemplate, moduleState.FolderName));
                    }
                    else
                    {
                        if (moduleState.SharedLogicModule)
                        {
                            body.AppendLine(string.Format(CmdSharedLogicLibDeleteTemplate, moduleState.FolderName));
                        }
                        else
                        {
                            body.AppendLine(string.Format(CmdRegularLibDeleteTemplate, moduleState.FolderName));
                        }
                    }
                }
            }

            File.WriteAllText(CmdFilePath, fileBeginText + "\n" + body + "\n" + fileEndText);
        }

        private void WriteShellFile()
        {
            var escapeDllHellFile = ShellFileTemplate;
            var startIndex = escapeDllHellFile.IndexOf(ShellFileBodyStartKey, StringComparison.InvariantCulture) +
                             ShellFileBodyStartKey.Length;
            var endIndex = escapeDllHellFile.IndexOf(ShellFileBodyEndKey, StringComparison.InvariantCulture);

            var fileBeginText = escapeDllHellFile.Substring(0, startIndex);
            var fileEndText = escapeDllHellFile.Substring(endIndex, escapeDllHellFile.Length - endIndex);

            StringBuilder body = new StringBuilder();
            foreach (var moduleState in _modules)
            {
                if (moduleState.Enabled)
                {
                    var relativePath = moduleState.Directory.Replace(Path.GetFullPath(_basePath), "").Replace("\\", "/");

                    if (moduleState.FolderName.EndsWith("Editor"))
                    {
                        body.AppendLine(string.Format(ShellEditorLibTemplate, moduleState.FolderName, relativePath).Replace("\\", "/"));
                    }
                    else
                    {
                        body.AppendLine(string.Format(ShellRegularLibTemplate, moduleState.FolderName, relativePath).Replace("\\", "/"));
                        if (moduleState.SharedLogicModule)
                        {
                            body.Append(string.Format(ShellSharedLogicLibTemplate, moduleState.FolderName, relativePath.Replace("\\", "/")));
                        }
                    }
                }
            }

            File.WriteAllText(ShellFilePath, fileBeginText + "\n" + body + "\n" + fileEndText);
        }

        public void ExecuteEscapeDllHell()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                ProcessStartInfo info =
                    new ProcessStartInfo(Path.GetFullPath(_basePath) + EscapeDllHellShellFile);
                info.UseShellExecute = true;
                info.Verb = "runas";
                Process.Start(info);
            }
            else if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ProcessStartInfo info =
                    new ProcessStartInfo(Path.GetFullPath(_basePath) + EscapeDllHellBatFile);
                info.UseShellExecute = true;
                info.Verb = "runas";
                Process.Start(info);
            }
        }
    }
}
