using System.IO;
using UnityEditor;

namespace PestelLib.PackageManager
{
    public class MatchmakerServerBuilder
    {
        private const string MatchmakerServerProjectName = "MatchmakerServer";
        private const string MatchmakerServerTemplateProjectName = MatchmakerServerProjectName + "Template";

        [MenuItem("PestelCrew/Server/Add MatchmakerServer")]
        public static void AddMatchmakerServer()
        {
            ConcreteSharedLogicBuilder.CloneTemplate(ServerTemplatePath, ConcreteServerPath, MatchmakerServerTemplateProjectName, MatchmakerServerProjectName);
            EscapeDllHellGenerator.ExecuteEscapeDllHell(); //run to rebuild solution PestelProjectLib
        }

        [MenuItem("PestelCrew/Server/Add MatchmakerServer", true)]
        public static bool AddConcreteSharedLogicValidate()
        {
            return !Directory.Exists(ConcreteServerPath);
        }

        private static string ConcreteServerPath
        {
            get { return Path.GetFullPath("./") + "ProjectLib/" + MatchmakerServerProjectName; }
        }

        private static string ServerTemplatePath
        {
            get { return Path.GetFullPath("./") + "PestelLib/MatchmakerServerTemplate"; }
        }
    }
}
