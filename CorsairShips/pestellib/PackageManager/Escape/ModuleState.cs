using System.Collections.Generic;

namespace PackageManager.Escape
{
    public class ModuleState
    {
        public string Name;
        public string Directory;
        public bool Enabled;
        public bool UserEnabled;
        public bool Foldout;
        public string FolderName;
        public List<string> Dependencies = new List<string>();
        public string LibraryType;
        public bool SharedLogicModule;
    }
}
