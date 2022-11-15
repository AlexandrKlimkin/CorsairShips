using CommandLine;

namespace PackageManager
{
    class Options
    {
        [Option("UpdateDependencies", Required = false, HelpText = "Don't show GUI: just update all dependencies")]
        public bool UpdateDependencies { get; set; }
    }
}
