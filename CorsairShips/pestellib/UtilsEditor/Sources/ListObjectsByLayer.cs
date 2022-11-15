using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PestelLib.UtilsEditor
{
    public class ListObjectsByLayer : EditorWindow
    {
        [MenuItem("PestelCrew/List Objects By Layer")]
        public static void ShowObjectsByLayer()
        {
            EditorWindow.GetWindow<ListObjectsByLayer>();
        }

        private int _selectedIndexLayer;
        private string[] _objectNames;

        private void OnGUI()
        {
            //Get layers
            var layers = new List<string>();
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    layers.Add(layerName);
                }
            }

            _selectedIndexLayer = EditorGUILayout.Popup(_selectedIndexLayer, layers.ToArray());

            if (GUILayout.Button("Find"))
            {
                _objectNames = GetObjectsByLayer(layers[_selectedIndexLayer]);
            }

            if (_objectNames != null && _objectNames.Length > 0)
            {
                for (int i = 0; i < _objectNames.Length; i++)
                {
                    GUILayout.Label(_objectNames[i]);
                }
            }
        }

        private string[] GetObjectsByLayer(string layerName)
        {
            var objectNames = new List<string>();
            var objectsInScene = FindObjectsOfType<Transform>();

            for (int i = 0; i < objectsInScene.Length; i++)
            {
                var obj = objectsInScene[i];
                if (obj.gameObject.layer == LayerMask.NameToLayer(layerName))
                {
                    if (!objectNames.Contains(obj.root.name))
                        objectNames.Add(obj.root.name);
                }
            }

            return objectNames.ToArray();
        }
    }


}