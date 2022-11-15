using System.IO;
using UnityEditor;
using UnityEngine;

namespace PestelLib.SharedLogicBase.Editor
{
    [CreateAssetMenu(fileName = "CommandGeneratorSettings", menuName = "Create shared logic generator settings")]
    public class CommandGeneratorSettings : ScriptableObject
    {
        private static CommandGeneratorSettings _instance;

        public static CommandGeneratorSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<CommandGeneratorSettings>("CommandGeneratorSettings");
                    if (_instance == null)
                    {
                        var newSettings = ScriptableObject.CreateInstance<CommandGeneratorSettings>();

                        var resourcesPath = Application.dataPath + "/Resources";
                        if (!Directory.Exists(resourcesPath))
                        {
                            Directory.CreateDirectory(resourcesPath);
                        }

                        AssetDatabase.CreateAsset(newSettings, "Assets/Resources/CommandGeneratorSettings.asset");
                        AssetDatabase.Refresh();
                        _instance = newSettings;
                    }
                }

                return _instance;
            }
        }

        [Header("location for .cs file to write AutoCommands (usually UserProfile.cs)")]
        public string ProtocolFilePath = "ProjectLib/ConcreteSharedLogic/Sources/UserProfile.cs";
        [Header("Main file in shared logic (usually SharedLogicCore.cs)")]
        public string SharedLogicCorePath = "ProjectLib/ConcreteSharedLogic/Sources/SharedLogicCore.cs";
        [Header("location for .cs file with generated commands")]
        public string WrapperFilePath = "Assets/CommandHelper.cs";
        [Header("AutoCommands namespace")]
        public string AutoCommandsNamespace = "S";
    }
}

