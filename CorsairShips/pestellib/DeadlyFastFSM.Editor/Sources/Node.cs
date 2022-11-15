using System;
using System.Collections.Generic;
using UnityEngine;

namespace DeadlyFast
{
    [System.Serializable]
    public class Node : ISerializationCallbackReceiver
    {
        public string Name;
        public Vector2 Position;
        public List<Transition> Transitions;

        [NonSerialized] public Rect Rect;
        [NonSerialized] public bool RenameInProcess;
        [NonSerialized] public string NameCandidate;

        public void OnBeforeSerialize()
        {
            Position = new Vector2(Rect.x, Rect.y);
        }

        public void OnAfterDeserialize()
        {
            Rect = new Rect(Position.x, Position.y, 100, 10);
        }

        public bool IsEntry
        {
            get { return Name == "Entry"; }
        }

        public bool IsAnyState
        {
            get { return Name == "AnyState"; }
        }

        public GUIStyle GetStyle(bool selected, bool initial)
        {
            if (IsAnyState)
            {
                return Utils.GetNodeStyle(Utils.NodeType.Any, selected);
            }

            if (IsEntry)
            {
                return Utils.GetNodeStyle(Utils.NodeType.Entry, selected);
            }

            if (initial)
            {
                return Utils.GetNodeStyle(Utils.NodeType.InitialState, selected);
            }

            return Utils.GetNodeStyle(Utils.NodeType.Default, selected);
        }
    }
}
