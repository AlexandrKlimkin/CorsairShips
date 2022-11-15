using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.IO;
using GoogleSpreadsheet;
using MessagePack;
using PestelLib.ServerShared;
using PestelLib.SharedLogicBase;
using S;
using UnityDI;

public class SharedLogicCommandDebugger : EditorWindow
{
    public UnityEngine.Object source;
    private string stateFileName = "?";
    private string requestFileName = "?";

    [MenuItem("PestelCrew/SharedLogic/Shared Logic Command Debugger")]
    static void Init()
    {
        var window = GetWindowWithRect<SharedLogicCommandDebugger>(new Rect(0, 0, 400, 100));
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(requestFileName);
        if (GUILayout.Button("Browse request.bin"))
        {
            requestFileName = EditorUtility.OpenFilePanel("Choose request.bin", "", "bin");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(stateFileName);
        if (GUILayout.Button("Browse state.bin"))
        {
            stateFileName = EditorUtility.OpenFilePanel("Choose state.bin", "", "bin");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("DefinitionContainer");
        source = EditorGUILayout.ObjectField(source, typeof(UnityEngine.Object), true);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Execute commands"))
        {
            if (source == null)
                ShowNotification(new GUIContent("No object selected for searching"));


            var go = source as GameObject;
            var components = go.GetComponents<MonoBehaviour>();

            foreach (var component in components)
            {
                var allFields = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var fieldInfo in allFields)
                {
                    if (fieldInfo.FieldType.GetInterfaces().Contains(typeof(IGameDefinitions)))
                    {
                        var definitionsType = fieldInfo.FieldType;
                        Debug.Log("Found definitions type: " + definitionsType);
                        var defs = fieldInfo.GetValue(component);
                        Debug.Log(JsonConvert.SerializeObject(defs));
                        Type sharedLogicType = null;

                        Type[] sharedLogicConstructorArgTypes = { typeof(S.UserProfile), definitionsType, typeof(IFeature) };

                        foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            sharedLogicType = a.GetType("PestelLib.SharedLogic.SharedLogicCore");
                            if (sharedLogicType != null) break;
                        }

                        var stateBytes = File.ReadAllBytes(stateFileName);
                        var userProfile = MessagePackSerializer.Deserialize<UserProfile>(stateBytes);

                        var slConstructor = sharedLogicType.GetConstructor(sharedLogicConstructorArgTypes);
                        var sharedLogic = (ISharedLogic)slConstructor.Invoke(new [] { userProfile, defs, null });

                        var cmdBytes = File.ReadAllBytes(requestFileName);
                        var cmd = MessagePackSerializer.Deserialize<ServerRequest>(cmdBytes);

                        var commandBatch = MessagePackSerializer.Deserialize<CommandBatch>(cmd.Data);
                        for (var i = 0; i < commandBatch.commandsList.Count; i++)
                        {
                            sharedLogic.Process<object>(commandBatch.commandsList[i]);
                        }
                        Debug.Log("Resulting state: " + sharedLogic.StateInJson());
                    }
                }
            }
        }
    }
}