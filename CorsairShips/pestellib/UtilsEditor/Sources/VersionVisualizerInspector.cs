// using PestelLib.Utils;
// using UnityEditor;
// using UnityEditor.Callbacks;
// using UnityEngine.UI;

// namespace PestelLib.UtilsEditor
// {
//     [CustomEditor(typeof(VersionVisualizer))]
//     public class VersionVisualizerInspector : Editor
//     {
//         [PostProcessScene]
//         public static void OnPostprocessScene()
//         {
//             var target = FindObjectOfType<VersionVisualizer>();
//             if (target != null)
//             {
//                 var text = target.GetComponent<Text>();
//                 text.text = "Ver. " + PlayerSettings.bundleVersion;
//                 EditorUtility.SetDirty(text);
//                 EditorUtility.SetDirty(target.gameObject);
//             }
//         }
//     }
// }