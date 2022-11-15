using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace PestelLib.ClientConfig.Editor.Sources
{
    [CustomEditor(typeof(Config), true)]
    public class ConfigEditor : UnityEditor.Editor
    {
        private string _statusText = "";
        private string _loadedConfig = "";

        public override void OnInspectorGUI()
        {
            var cfg = (Config)target;

            Preset(cfg, "Production", "config.production");
            Preset(cfg, "Production iOS", "config.production.ios");
            Preset(cfg, "Development", "config.development");
            Preset(cfg, "QA", "config.qa");
            Preset(cfg, "My Development A", "config.development.a");
            Preset(cfg, "My Development B", "config.development.b");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(_statusText, EditorStyles.boldLabel);
            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }

        private void Preset(Config cfg, string presetName, string filename)
        {
            EditorGUILayout.BeginHorizontal();

            if (presetName == _loadedConfig) {
                EditorGUILayout.LabelField(presetName, EditorStyles.boldLabel);
            } else {
                EditorGUILayout.LabelField(presetName);
            }

            EditorGUI.BeginDisabledGroup(presetName == _loadedConfig);
            if (GUILayout.Button("Load"))
            {
                ResetProperties();

                LoadConfig(filename, cfg);
                EditorUtility.SetDirty(cfg);

                _statusText = "Loaded config: " + presetName;
                _loadedConfig = presetName;
            }
            EditorGUI.EndDisabledGroup ();

            //EditorGUI.BeginDisabledGroup(presetName != _loadedConfig);
            if (GUILayout.Button("Save"))
            {
                var configJson = Resources.Load<TextAsset>(filename);
                var path = string.Empty;
                if (configJson != null)
                {
                    path = AssetDatabase.GetAssetPath(configJson);
                }
                else
                {
                    path = Application.dataPath + "/Resources/" + filename + ".json";
                }
                File.WriteAllText(path, JsonConvert.SerializeObject(cfg, Formatting.Indented));
                AssetDatabase.Refresh();
                _statusText = "Config saved to file: " + path;
            }
            //EditorGUI.EndDisabledGroup ();

            EditorGUILayout.EndHorizontal();
        }

        private void ResetProperties()
        {
            var cleanInstance = ScriptableObject.CreateInstance(target.GetType());
            var cleanInstanceValues = JsonConvert.SerializeObject(cleanInstance);
            JsonConvert.PopulateObject(cleanInstanceValues, target);
            DestroyImmediate(cleanInstance);
        }

        public static void LoadConfig(string configName, Config config)
        {
            var configJson = Resources.Load<TextAsset>(configName);
            if (configJson == null)
            {
                Debug.Log(string.Format("Config Resources/{0} not found, using default values", configName));
            }
            else
            {
                try
                {
                    config.PhotonServers.Clear();
                    JsonConvert.PopulateObject(configJson.text, config);
                    Debug.Log("Conflig loaded from: '" + configName + "' " + configJson.text);
                }
                catch (Exception e)
                {
                    Config.ParseError(e, "config.json");
                }
            }
        }

        public static void LoadConfigFromString(string configPath, Config config)
        {
            var configJsonText = File.OpenText(configPath).ReadToEnd();
            JsonConvert.PopulateObject(configJsonText, config);
        }

        public static Config FindConfig() {
            var configGUID = AssetDatabase.FindAssets("t:config")[0];
            var configPath = AssetDatabase.GUIDToAssetPath(configGUID);
            var config = AssetDatabase.LoadAssetAtPath<Config>(configPath);

            return config;
        }

        public static Config[] FindConfigs()
        {
            var configGUIDs = AssetDatabase.FindAssets("t:config");
            Debug.Log("Configs guids: " + string.Join(", ", configGUIDs));
            var configPaths = configGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            var configs = configPaths.Select(AssetDatabase.LoadAssetAtPath<Config>);

            return configs.ToArray();
        }

        public static Config FindConfig(Type t)
        {
            var configGUIDs = AssetDatabase.FindAssets("t:" + t.Name);
            Debug.Log("Configs guids: " + string.Join(", ", configGUIDs));
            var configPaths = configGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            var configs = configPaths.Select(s => AssetDatabase.LoadAssetAtPath(s, t)).OfType<Config>();

            return configs.FirstOrDefault();
        }
    }


    public class ClientConfigWindow : EditorWindow
    {
        Config config;
        UnityEditor.Editor editor;
        private Vector2 _scrollView;

        [MenuItem("PestelCrew/Client Config")]
        static void Init()
        {
            var window = (ClientConfigWindow)EditorWindow.GetWindow(typeof(ClientConfigWindow));
            window.titleContent = new GUIContent("Client config");
            window.Show();
        }

        void OnEnable() {
            var configs = Resources.FindObjectsOfTypeAll<Config>();
            
            config = configs[0];
            editor = UnityEditor.Editor.CreateEditor(config);
        }

        void OnGUI()
        {
            EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth * 0.4f;
            _scrollView = EditorGUILayout.BeginScrollView(_scrollView);
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical();

            UnityEditor.Editor.CreateCachedEditor(config, editor.GetType(), ref editor);
            editor.DrawHeader();
            editor.OnInspectorGUI();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
        }
    }
}