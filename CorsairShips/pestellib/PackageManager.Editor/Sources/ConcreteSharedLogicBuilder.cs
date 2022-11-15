using System.IO;
using UnityEditor;

namespace PestelLib.PackageManager
{
    public class ConcreteSharedLogicBuilder
    {
        private const string ConcreteSharedLogicProjectName = "ConcreteSharedLogic";

        [MenuItem("PestelCrew/SharedLogic/Add ConcreteSharedLogic")]
        public static void AddConcreteSharedLogic()
        {
            CloneTemplate(SharedLogicTemplatePath, ConcreteSharedLogicPath, "SharedLogicTemplate", ConcreteSharedLogicProjectName);
        }

        public static void CloneTemplate(string sourcePath, string destinationPath, string sourceProjectFileName, string destinationProjectFileName)
        {
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            DirectoryCopy(sourcePath, destinationPath, true);

            var projectDestinationPath = destinationPath + Path.DirectorySeparatorChar + destinationProjectFileName + ".csproj";

            File.Move(
                destinationPath + Path.DirectorySeparatorChar + sourceProjectFileName + ".csproj",
                projectDestinationPath
            );

            var projectContent = File.ReadAllText(projectDestinationPath);
            projectContent = projectContent.Replace("rem $(SolutionDir)", "$(SolutionDir)");
			var newPath = Path.Combine(Path.Combine("..", ".."), "pestellib");
            projectContent = projectContent.Replace("ProjectReference Include=\"..", "ProjectReference Include=\"" + newPath);
			var sharedLogic = Path.Combine(Path.Combine(newPath, "SharedLogicTemplate"),"SharedLogicTemplate.csproj");
			projectContent = projectContent.Replace(sharedLogic, Path.Combine("..", Path.Combine("ConcreteSharedLogic","ConcreteSharedLogic.csproj")));
            File.WriteAllText(projectDestinationPath, projectContent);

            OpenInFileBrowser.Open(destinationPath);
        }

        [MenuItem("PestelCrew/SharedLogic/Add ConcreteSharedLogic", true)]
        public static bool AddConcreteSharedLogicValidate()
        {
            return !Directory.Exists(ConcreteSharedLogicPath);
        }

        private static string ConcreteSharedLogicPath
        {
            get { return Path.GetFullPath("./") + "ProjectLib/" + ConcreteSharedLogicProjectName; }
        }

        private static string SharedLogicTemplatePath
        {
            get { return Path.GetFullPath("./") + "PestelLib/SharedLogicTemplate"; }
        }

#region CopyDirectory
        //WTF??? https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
#endregion
    }
}