using System;
using System.Collections.Generic;
using System.Linq;
using DeadlyFast;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace DeadlyFast
{
    [Serializable]
    [CreateAssetMenu(
        fileName = "New FSM data",
        menuName = "Deadly Fast FSM")]
    public class FsmData : ScriptableObject
    {
        public List<Node> Nodes;

        public Node NodeByName(string name)
        {
            return Nodes.FirstOrDefault(x => x.Name == name);
        }

        [OnOpenAsset(1)]
        public static bool HandleOpen(int instanceID, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceID).GetType() == typeof (FsmData))
            {
                EditorWindow.GetWindow(typeof(DeadlyFastFSM));
                return true;
            }
            return false;
        }
    }
}
