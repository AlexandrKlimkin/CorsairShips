using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ServerShared.GlobalConflict;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GlobalConflictModule.Scripts
{
    [CustomEditor(typeof(GlobalConflictDesc))]
    public class GlobalConflictDescEditor : Editor
    {
        public bool _showFilesToLoad;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            if(!Application.isPlaying)
                DrawInEditor();
        }

        private void DrawInEditor()
        {
            var e = (GlobalConflictDesc)target;
            if (GUILayout.Button("AddNode"))
            {
                e.AddNode();
            }

            if (GUILayout.Button("AddDonation"))
            {
                e.AddDonationLevel();
            }

            if (GUILayout.Button("AddPointOfInterest"))
            {
                e.AddPointOfInterest();
            }

            if (GUILayout.Button("AddQuest"))
            {
                e.AddQuest();
            }

            if (GUILayout.Button("Reset"))
            {
                e.Reset();
            }

            if (GUILayout.Button("Export"))
            {
                var validator = new GlobalConflictValidatorDummy();
                var messages = new ValidatorMessageCollection();
                var state = e.GenerateState();
                validator.IsValid(state, messages);
                foreach (var s in messages)
                {
                    if (s.Level == MessageLevel.Error)
                        Debug.LogError(s.Message);
                    else
                        Debug.LogWarning(s.Message);
                }

                if (messages.Errors == 0)
                {
                    var fix = 0;
                    var filePath = Path.Combine(Application.persistentDataPath, state.Id + ".json");
                    while (File.Exists(filePath))
                    {
                        filePath = Path.Combine(Application.persistentDataPath, state.Id + "_" + (++fix) + ".json");
                    }

                    var data = JsonConvert.SerializeObject(state, Formatting.Indented);
                    using (var f = new StreamWriter(File.OpenWrite(filePath)))
                    {
                        f.Write(data);
                    }

                    Debug.Log("Conflict exported to: " + filePath);
                }
            }

            if (GUILayout.Button("Load"))
            {
                _showFilesToLoad ^= true;
            }

            if (GUILayout.Button("PrintSize"))
            {
                var nodes = e.GetComponentsInChildren<PlacableNodeDesc>();
                foreach (var node in nodes)
                {
                    var p = node.GetRelativePosition();
                    Debug.Log(node.name + ": " + p);
                }
            }

            if (GUILayout.Button("Clear"))
            {
                Debug.Log("Child count " + e.transform.childCount);
                var cs = e.transform.Cast<Transform>();
                foreach (Transform transform in e.transform)
                {
                    Debug.Log("Destroy " + transform.name);
                    DestroyImmediate(transform.gameObject);
                }
            }

            if (_showFilesToLoad)
            {
                var msg = "Files from '" + Application.persistentDataPath + "/*.json'";
                EditorGUILayout.LabelField(msg, GUI.skin.box, GUILayout.ExpandWidth(true));
                var jsons = Directory.GetFiles(Application.persistentDataPath, "*.json");
                foreach (var jsonFile in jsons)
                {
                    var shortName = Path.GetFileName(jsonFile);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Label(shortName);

                        if (GUILayout.Button("Load"))
                        {
                            var data = File.ReadAllText(jsonFile);
                            try
                            {
                                var state = JsonConvert.DeserializeObject<GlobalConflictState>(data);
                                e.Restore(state);
                            }
                            catch (Exception exception)
                            {
                                Debug.LogError("Bad format. " + exception);
                            }
                        }
                    }
                }
            }
        }
    }
}