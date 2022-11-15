using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PestelLib.ClientConfig;
using PestelLib.Serialization;
using PestelLib.ServerShared;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GoogleSpreadsheet;
using Debug = UnityEngine.Debug;

namespace PestelLib.GoogleSpreadsheet.Editor
{
    public struct DefToValidate {
        public string name;
        public IList oldList;
        public IList newList;
    }

    public class GoogleImporterWindow : EditorWindow
    {
        private const string DefaultServerPathKey = "PathToServer";

        private Dictionary<string, bool> _pagesCheckBoxes;
        private Dictionary<string, bool> _importResults = new Dictionary<string, bool>();
        private GameObject _selection;

        private bool _isControlsEnabled = true;

        private static string _targetDir;

        private static List<DefToValidate> defsToValidate = new List<DefToValidate>();

        private static IGameDefinitions newDefs;

        [MenuItem("PestelCrew/Import Google Data")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            var window = (GoogleImporterWindow)EditorWindow.GetWindow(typeof(GoogleImporterWindow));
            window.titleContent = new GUIContent("Import From Google");
            window.Show();
        }

        private GameObject _prefab;

        void OnEnable()
        {
            _prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EditorPrefs.GetString("Definitions path"));
            _selection = null;
        }

        void OnDisable()
        {
            if (_pagesCheckBoxes != null)
            {
                foreach (var pagesCheckBox in _pagesCheckBoxes)
                {
                    EditorPrefs.SetBool(pagesCheckBox.Key, pagesCheckBox.Value);
                }
            }
            if (_prefab != null)
            {
                var prefabPath = AssetDatabase.GetAssetPath(_prefab);
                EditorPrefs.SetString("Definitions path", prefabPath);
            }
        }

        private Vector2 _scrollView;
        void OnGUI()
        {
            _prefab = (GameObject)EditorGUILayout.ObjectField("Definitions", _prefab, typeof(GameObject), true);
            if (_selection != _prefab)
            {
                _selection = _prefab;
                UpdateObjects();
            }

            if (_selection == null)
            {
                GUILayout.Label("Select object");
                return;
            }

            if (_pagesCheckBoxes == null || _pagesCheckBoxes.Count == 0)
            {
                GUILayout.Label("Current object has no fields to work with");
                return;
            }

            GUI.enabled = _isControlsEnabled;

            ShowControls();
            _scrollView = EditorGUILayout.BeginScrollView(_scrollView);
            ShowList();
            EditorGUILayout.EndScrollView();
        }

        private void ShowControls()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("All"))
            {
                var keys = new List<string>(_pagesCheckBoxes.Keys);
                foreach (var key in keys)
                {
                    _pagesCheckBoxes[key] = true;
                }
            }

            if (GUILayout.Button("None"))
            {
                var keys = new List<string>(_pagesCheckBoxes.Keys);
                foreach (var key in keys)
                {
                    _pagesCheckBoxes[key] = false;
                }
            }

            if (GUILayout.Button("Import"))
            {
                ImportData();
            }

            GUILayout.EndHorizontal();
        }

        private void ShowList()
        {
            GUILayout.BeginVertical();
            var keys = new List<string>(_pagesCheckBoxes.Keys);
            keys.Sort();
            var defaultColor = GUI.color;
            foreach (var key in keys)
            {
                if (!_importResults.ContainsKey(key) || _importResults[key])
                    GUI.color = defaultColor;
                else
                    GUI.color = Color.red;

                _pagesCheckBoxes[key] = EditorGUILayout.Toggle(key, _pagesCheckBoxes[key]);
            }
            GUI.color = defaultColor;

            GUILayout.EndVertical();
        }

        public void OnInspectorUpdate()
        {
            // This will only get called 10 times per second.
            Repaint();
        }

        private void UpdateObjects()
        {
            if (_selection == null) return;

            _isControlsEnabled = false;
            _pagesCheckBoxes = new Dictionary<string, bool>();

            foreach (MonoBehaviour monoBehaviour in _selection.GetComponents<MonoBehaviour>())
            {
                ParseFields("", monoBehaviour, (uri, attr, obj, shouldUpdateConfig) =>
                {
                    if (!_pagesCheckBoxes.ContainsKey(attr.PageName))
                    {
                        _pagesCheckBoxes.Add(attr.PageName, EditorPrefs.GetBool(attr.PageName));
                    }
                });
            }
            _isControlsEnabled = true;
        }

        private ImportQueue _tasksQueue;
        private void ImportData()
        {
            _isControlsEnabled = false;

            var progressBar = new EditorProgressBar("Import from google in progress");
            _tasksQueue = ImportQueue.CreateQueue(LoadFromGoogle, progressBar);

            _tasksQueue.OnComplete += OnParsingComplete;
            newDefs = MakeDefsCopy(_prefab.GetComponent<MonoBehaviour>());
            bool shouldUpdateConfig = false;
            foreach (MonoBehaviour monoBehaviour in _selection.GetComponents<MonoBehaviour>())
            {
                ParseFields("", monoBehaviour, (uri, attr, obj, updateConfig) =>
                {
                    if (_pagesCheckBoxes[attr.PageName]) {
                        _tasksQueue.AddTask(new ImportPageData(uri, attr.PageName, obj, null));
                        shouldUpdateConfig |= updateConfig; 
                    }
                });
            }
            if (shouldUpdateConfig)
                UpdateConfig();
        }

        private void OnParsingComplete()
        {
            HashSet<object> duplicateChecker = new HashSet<object>();
            var errorsList = new StringBuilder();
            bool hasDuplicate = false;

            foreach (var toValidate in defsToValidate)
            {
                _importResults[toValidate.name] = true;
                var defs = toValidate.newList;
                var fi = GetFieldInfo(defs.GetType(), newDefs);
                if (fi != null)
                    fi.SetValue(newDefs, defs);

                var rowNumber = 0;
                foreach (var def in defs)
                {
                    rowNumber++;
                    var stringIdField = def.GetType().GetField("Id");
                    if (stringIdField != null)
                    {
                        if (stringIdField.FieldType == typeof(string))
                        {
                            string id = (string) stringIdField.GetValue(def);
                            if (!duplicateChecker.Add(id))
                            {
                                hasDuplicate = true;
                                _importResults[toValidate.name] = false;
                                errorsList.AppendFormat("Duplicate in {0} Id: {1} Content: {2}\n Row: {3}",
                                    def.GetType().Name, id, JsonConvert.SerializeObject(def), rowNumber);
                            }
                        }
                        else if (stringIdField.FieldType == typeof(int))
                        {
                            int id = (int) stringIdField.GetValue(def);
                            if (!duplicateChecker.Add(id))
                            {
                                hasDuplicate = true;
                                _importResults[toValidate.name] = false;
                                errorsList.AppendFormat("Duplicate in {0} Id: {1} Content: {2}\n Row: {3}",
                                    def.GetType().Name, id, JsonConvert.SerializeObject(def), rowNumber);
                            }
                        }
                    }
                }

                duplicateChecker.Clear();
            }

            if (hasDuplicate)
            {
                Debug.LogError(errorsList);
                EditorUtility.DisplayDialog("Duplicate def Id Error", errorsList.ToString(), "Fuck...");
            }

            newDefs.OnAfterDeserialize();

            foreach(var toValidate in defsToValidate) {
                if (toValidate.oldList.Count > 0 && toValidate.oldList[0] is IValidatableDef) {
                    string err;
                    if (!((IValidatableDef)toValidate.oldList[0]).Validate(toValidate.oldList, toValidate.newList, newDefs, out err)) {
                        Debug.LogError(err);
                        EditorUtility.DisplayDialog("Error", err, "Fuck...");
                        _importResults[toValidate.name] = false;
                        continue;
                    }
                    _importResults[toValidate.name] = _importResults[toValidate.name];
                }
                toValidate.oldList.Clear();
                foreach(var o in toValidate.newList) {
                    toValidate.oldList.Add(o);
                }
            }

            defsToValidate.Clear();
            
            _tasksQueue.OnComplete -= OnParsingComplete;
            _tasksQueue = null;

            _isControlsEnabled = true;
            EditorUtility.SetDirty(_prefab);
            AssetDatabase.SaveAssets();
        }

        private void UpdateConfig()
        {
            var sharedConfigPath = Application.dataPath + "/Resources/sharedConfig.json";

            var config = new SharedConfig();
            if (File.Exists(sharedConfigPath))
            {
                var configTxt = File.ReadAllText(sharedConfigPath);
                config = JsonConvert.DeserializeObject<SharedConfig>(configTxt);
            }

            config.DefinitionsVersion = config.DefinitionsVersion + 1;
            var serializedConfig = JsonConvert.SerializeObject(config, Formatting.Indented);

            var dirPath = Application.dataPath + "/Resources";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            File.WriteAllText(sharedConfigPath, serializedConfig);

            WriteConfigToServer(Application.dataPath + "/../pestellib/Backend2/", serializedConfig);
            WriteConfigToServer(Application.dataPath + "/../pestellib/CoreBackend/", serializedConfig);
        }

        private void WriteConfigToServer(string backendPath, string serializedConfig)
        {
            //var backendPath = Application.dataPath + "/../pestellib/Backend2/";
            var backedAppData = backendPath + "App_Data/";
            if (Directory.Exists(backendPath))
            {
                Directory.CreateDirectory(backedAppData);
                var serverConfigPath = backedAppData + "sharedConfig.json";
                File.WriteAllText(serverConfigPath, serializedConfig);
            }
        }

        IGameDefinitions MakeDefsCopy(object obj) {

            var bindingFlags = BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public |
                               BindingFlags.NonPublic;

            var fields = obj.GetType().GetFields(bindingFlags);

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo fieldInfo = fields[i];
                if (!Implements(fieldInfo.FieldType, typeof(IGameDefinitions))) continue;

                var oldDefs = fieldInfo.GetValue(obj);

                using (var ms = new MemoryStream())
                {
                  var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                  formatter.Serialize(ms, oldDefs);
                  ms.Position = 0;

                  return (IGameDefinitions) formatter.Deserialize(ms);
                }
            }
            return null;
        }

        public static bool Implements(Type candidateType, Type interfaceType)
        {
            if (interfaceType.IsInterface && !candidateType.IsInterface && !candidateType.IsAbstract)
                return interfaceType.IsAssignableFrom(candidateType);
            return false;
        }

        FieldInfo GetFieldInfo(Type type, IGameDefinitions defs) {
            var bindingFlags = BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var fields = defs.GetType().GetFields(bindingFlags);

            return fields.FirstOrDefault(fi => fi.FieldType == type);
        }

        private void ParseFields(string uri, object obj, Action<string, GooglePageRefAttribute, object, bool> onFieldFound, bool shouldUpdateConfig = false, int depthStep = 0)
        {
            if (depthStep > 4) return;

            var bindingFlags = BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public |
                               BindingFlags.NonPublic;

            var fields = obj.GetType().GetFields(bindingFlags);

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo fieldInfo = fields[i];
                bool shouldUpdate = shouldUpdateConfig;
                if (Attribute.IsDefined(fieldInfo, typeof(GoogleSpreadsheetUriAttribute)))
                {
                    var attr =
                        (GoogleSpreadsheetUriAttribute)
                            Attribute.GetCustomAttribute(fieldInfo, typeof(GoogleSpreadsheetUriAttribute));
                    uri = attr.Uri;
                    shouldUpdate = !string.IsNullOrEmpty(attr.CopyTo);

                    if (!string.IsNullOrEmpty(attr.CopyTo))
                        _targetDir = attr.CopyTo;
                }

                if (Attribute.IsDefined(fieldInfo, typeof(GooglePageRefAttribute)))
                {                    
                    var attr =
                        (GooglePageRefAttribute)
                            Attribute.GetCustomAttribute(fieldInfo, typeof(GooglePageRefAttribute));
                    var target = fieldInfo.GetValue(obj);
                    if (target == null)
                    {
                        target = Activator.CreateInstance(fieldInfo.FieldType);
                        fieldInfo.SetValue(obj, target);
                    }

                    if (!(target is IList))
                    {
                        continue;
                    }
                    onFieldFound(uri, attr, target, shouldUpdate);
                }
                else
                {
                    if (fieldInfo.GetValue(obj) != null)
                        ParseFields(uri, fieldInfo.GetValue(obj), onFieldFound, shouldUpdate, depthStep + 1);
                }
            }
        }

        private static IEnumerator LoadFromGoogle(ImportPageData data)
        {
            var googleAppUrl = data.PageUri;
            if (string.IsNullOrEmpty(googleAppUrl))
            {
                Debug.LogError("Give me link to the google api page. Add attribute to root element like this: [GoogleSpreadsheetUri(\"https://script.google.com/macros/s/AKfycbzwd-Ik4Zp0sE1Mpr2aFcV5V75PsLogbMO8oTVutfupT5cNTe1s/exec\")]");
                yield break;
            }

            var url = !googleAppUrl.Contains("?")
                ? string.Format(googleAppUrl + "?page={0}", data.PageName)
                : string.Format(googleAppUrl + "&page={0}", data.PageName);

            var request = new WWW(url);

            while (!request.isDone)
            {
                yield return null;
            }

            var defs = (IList)data.Target;

            var newDefs = (IList)Activator.CreateInstance(defs.GetType());

            defsToValidate.Add(new DefToValidate() {
                name = data.PageName,
                oldList = defs,
                newList = newDefs
            });

            Debug.Log(data.PageName + ": " + request.text);

            try
            {
                DefsParser.PopulateObject(request.text, newDefs);
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("Exception during import page " + data.PageName, e.Message, "Fuck...");
                Debug.LogError(e.Message + e.InnerException + e.StackTrace);
                yield break;
            }

            if (!string.IsNullOrEmpty(_targetDir))
            {
                var path = Application.dataPath + _targetDir;

                Directory.CreateDirectory(path);
                File.WriteAllText(path + data.PageName + ".json", JsonConvert.SerializeObject(newDefs, Formatting.Indented));

                var backendPath = Application.dataPath + "/../pestellib/Backend2/";
                var backedAppData = backendPath + "App_Data/";
                if (Directory.Exists(backendPath))
                {
                    Directory.CreateDirectory(backedAppData);
                    File.WriteAllText(backedAppData + data.PageName + ".json", JsonConvert.SerializeObject(newDefs, Formatting.Indented));
                }
                else
                {
                    Debug.LogError(string.Format("Backend App_Data not found. Please check path '{0}'", backendPath));
                }
            }
        }
    }
}