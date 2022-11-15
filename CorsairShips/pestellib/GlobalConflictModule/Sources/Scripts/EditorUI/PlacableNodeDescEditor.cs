using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GlobalConflictModule.Scripts
{
    [CustomEditor(typeof(PlacableNodeDesc))]
    [CanEditMultipleObjects]
    public class PlacableNodeDescEditor : Editor
    {
        private PlacableNodeDesc[] SelectedNodes
        {
            get
            {
                return Selection.objects.OfType<GameObject>().Select(_ => _.GetComponent<PlacableNodeDesc>())
                    .Where(_ => _ != null).ToArray();
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (!Application.isPlaying)
            {
                DrawInEditor();
            }
        }

        private void DrawInEditor()
        {
            if (GUILayout.Button("LinkNodes"))
            {
                var nodes = SelectedNodes;
                if (nodes.Length < 2)
                {
                    Debug.LogError("Select 2 or more nodes");
                    return;
                }

                for (var i = 0; i < nodes.Length; ++i)
                {
                    for (var j = 0; j < nodes.Length; ++j)
                    {
                        if (i == j)
                            continue;
                        nodes[i].AddLink(nodes[j]);
                    }
                }
            }

            if (GUILayout.Button("UnlinkNodes"))
            {
                var nodes = SelectedNodes;
                if (nodes.Length < 2)
                {
                    Debug.LogError("Select 2 or more nodes");
                    return;
                }

                for (var i = 0; i < nodes.Length; ++i)
                {
                    for (var j = 0; j < nodes.Length; ++j)
                    {
                        if (i == j)
                            continue;
                        nodes[i].RemoveLink(nodes[j]);
                    }
                }
            }
        }
    }
}