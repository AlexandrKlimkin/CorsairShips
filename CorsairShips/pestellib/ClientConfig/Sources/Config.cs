using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityDI;
using UnityEngine;

namespace PestelLib.ClientConfig
{

    [CreateAssetMenu(fileName = "Config", menuName = "Pestel Config", order = 1)]
    public class Config : ScriptableObject, IDependent
    {
        [Header("Debug")]
        public bool UseLocalState = true;

        public bool IsInExperimentalGroup;
        public bool DebugWriteRequestsOnDisk = false;
        public bool UseJsonSerialization = false;

        [Header("Shared Logic: CommandProcessor")]
        public bool DontPackMultipleCommands = false;
        public int CommandsBatchSize = 1;
        public float CommandsSyncTimeout = 0f;
        public bool EnableOfflineSharedLogic = false;
        public bool DontCheckDefinitionsEquality = false;

        public bool EnableMultiInstance; // чтоб использовались разные playerId при запуске нескольких инстансов на одной машине

        [Header("Shared Logic Sever")]
        public string DynamicURL = "http://localhost:50626/api";
        public string IapValidatorUrl = "http://localhost/validateReceipt";

        [Header("Config override")]
        public string ConfigOverrideBaseURL = "http://localhost/config";

        [Header("Matchmaker")]
        public bool UseMatchmaker = false;
        public string MatchmakerURL = "localhost:8500";

        public SharedConfig SharedConfig;

        [Header("Photon")]
        public string PhotonServerVersion = "pc_1.10.107dev";
        public string PhotonPluginVersion = "v0.29.4";
        public List<PhotonServerDescription> PhotonServers = new List<PhotonServerDescription>()
        {
            new PhotonServerDescription
            {
                Name = "EU",
                Uri = "mm.planetcommander.ru",
                Port = 5055
            },
            new PhotonServerDescription
            {
                Name = "US",
                Uri = "mmphotonus.southcentralus.cloudapp.azure.com",
                Port = 5055
            }
        };

        public string BuildSignature = "23:AA:19:D5:2F:F0:D4:CF:4D:05:DE:9F:D0:35:FA:52:ED:97:C9:B5";
        public bool UseIntegrityCheck = true;

        [Header("Log server")]
        public bool LogServerEnabled = false;
        public string LogServer = string.Empty;
        public bool LogCollectUnityErrors = false;

        [Header("Asset bundles")]
        public string AssetBundleUrl;
        public string EditorAssetBundleUrl; 
        public bool LoadBundlesFromFileSystem = false;
        public bool DontLoadAssetBundles = false;
        public bool LoadBundlesFromStreamingAssets = false;
        public int BundlesVersion = 0;

        [Header("Support")]
        public string SupportEmail = "support@gdcgames.ru";

        [Header("FriendsServer")]
        public bool FriendsServerEnabled = false;
        public string FriendsServerUrl = "tcp://localhost:9001";

        [Header("ClansServer")]
        public bool ClansServerEnabled = false;
        public string ClansServerUrl = "tcp://localhost:10000";

        [Header("VoiceChat")]
        public bool UseUnityMicrophoneApi = false;
        public bool UseVoiceDetector = false;

        [Header("PhotonBolt")]
        public string[] LoadBalancingIP = {"127.0.0.1"};

        public static void ParseError(Exception e, string configName)
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorUtility.DisplayDialog(
                "Parse error in " + configName,
                e.Message + "\nFix error and hit Play button again",
                "Stop Game"
                ))
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
#else
            Debug.LogError("Can't parse " + configName + " " + e.Message);
#endif
        }
        
        public void MergeWith(string config)
        {
            JsonConvert.PopulateObject(config, this);
        }

        public void OnInjected()
        {
            var sharedConfigJson = Resources.Load<TextAsset>("sharedConfig");
            if (sharedConfigJson == null)
            {
                Debug.LogError("sharedConfig.json not found in resources folder!");
                return;
            }

            Debug.Log(sharedConfigJson.text);
            try
            {
                SharedConfig = JsonConvert.DeserializeObject<SharedConfig>(sharedConfigJson.text);
            }
            catch (Exception e)
            {
                ParseError(e, "sharedConfig.json");
            }
        }
    }
}
