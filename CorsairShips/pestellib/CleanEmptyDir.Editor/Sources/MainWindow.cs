using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace AltProg.CleanEmptyDir
{
    public class MainWindow : EditorWindow
    {
        List<string> emptyDirs;
        Vector2 scrollPosition;
        bool lastCleanOnSave;
        string delayedNotiMsg;
        UpdateChecker.Message updateMsg;
        GUIStyle updateMsgStyle;

        bool hasNoEmptyDir { get { return emptyDirs == null || emptyDirs.Count == 0; } }

        const float DIR_LABEL_HEIGHT = 21;

        [MenuItem("Window/AltProg Clean Empty Dir")]
        public static void ShowWindow()
        {
            var w = GetWindow<MainWindow>();
            var titleContent = w.titleContent;
            titleContent.text = "Clean";
        }

        void OnEnable()
        {
            lastCleanOnSave = Core.CleanOnSave;
            Core.OnAutoClean += Core_OnAutoClean;
            UpdateChecker.OnDone += UpdateChecker_OnDone;

            UpdateChecker.Check();
            delayedNotiMsg = "Click 'Find Empty Dirs' Button.";
        }
        
        void OnDisable()
        {
            Core.CleanOnSave = lastCleanOnSave;
            Core.OnAutoClean -= Core_OnAutoClean;
            UpdateChecker.OnDone -= UpdateChecker_OnDone;
        }

        void UpdateChecker_OnDone( UpdateChecker.Message concreteUpdateMsg )
        {
            updateMsg = concreteUpdateMsg;
        }

        void Core_OnAutoClean()
        {
            delayedNotiMsg = "Cleaned on Save";
        }

        void OnGUI()
        {
            if ( delayedNotiMsg != null )
            {
                ShowNotification( new GUIContent( delayedNotiMsg ) );
                delayedNotiMsg = null;
            }

            EditorGUILayout.BeginVertical();
            {
                if ( null != updateMsg )
                {
                    if ( updateMsgStyle == null )
                    {
                        updateMsgStyle = new GUIStyle("CN EntryInfo")
                        {
                            alignment = TextAnchor.MiddleLeft,
                            richText = true
                        };
                    }

                    if ( GUILayout.Button( updateMsg.Msg , updateMsgStyle) )
                    {
                        Application.OpenURL( updateMsg.Link );
                    }
                }

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Find Empty Dirs"))
                    {
                        Core.FillEmptyDirList(out emptyDirs);

                        if (hasNoEmptyDir)
                        {
                            ShowNotification( new GUIContent( "No Empty Directory" ) );
                        }
                        else
                        {
                            RemoveNotification();
                        }
                    }

                    if ( ColorButton( "Delete All", ! hasNoEmptyDir, Color.red ) )
                    {
                        Core.DeleteAllEmptyDirAndMeta(ref emptyDirs);
                        ShowNotification( new GUIContent( "Deleted All" ) );
                    }
                }
                EditorGUILayout.EndHorizontal();    


                var cleanOnSave = GUILayout.Toggle(lastCleanOnSave, " Clean Empty Dirs Automatically On Save");
                if (cleanOnSave != lastCleanOnSave)
                {
                    lastCleanOnSave = cleanOnSave;
                    Core.CleanOnSave = cleanOnSave;
                }

                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));

                if ( ! hasNoEmptyDir )
                {
                    scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true));
                    {
                        EditorGUILayout.BeginVertical();
                        {
                            var assetObj = AssetDatabase.LoadAssetAtPath( "Assets", typeof(UnityEngine.Object) );

                            foreach (var dirInfo in emptyDirs)
                            {
                                if ( null != assetObj )
                                {
                                    GUILayout.Label( dirInfo, GUILayout.Height( DIR_LABEL_HEIGHT ) );
                                }
                            }

                        }
                        EditorGUILayout.EndVertical();

                    }
                    EditorGUILayout.EndScrollView();
                }

            }
            EditorGUILayout.EndVertical();
        }


        void ColorLabel(string labelTitle, Color color)
        {
            var oldColor = GUI.color;
            //GUI.color = color;
            GUI.enabled = false;
            GUILayout.Label(labelTitle);
            GUI.enabled = true;
            GUI.color = oldColor;
        }
        
        bool ColorButton(string labelTitle, bool enabled, Color color)
        {
            var oldEnabled = GUI.enabled;
            var oldColor = GUI.color;

            GUI.enabled = enabled;
            GUI.color = color;

            var ret = GUILayout.Button(labelTitle);

            GUI.enabled = oldEnabled;
            GUI.color = oldColor;
            
            return ret;
        }
    }

}
