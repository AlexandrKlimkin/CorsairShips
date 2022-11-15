using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DeadlyFast
{
    public static class Utils
    {
        public static void DrawNodeCurve(Rect start, Rect end, int eventIndex, Color color)
        {
            Vector3 startPos;
            Vector3 startTan;

            var verticalStartPos = start.y + eventIndex*21 + 30;
            if (start.center.x < end.center.x)
            {
                startPos = new Vector3(start.x + start.width, verticalStartPos, 0);
                startTan = startPos + Vector3.right*50;
            }
            else
            {
                startPos = new Vector3(start.x, verticalStartPos, 0);
                startTan = startPos + Vector3.left*50;
            }

            Vector3 endPos = new Vector3(end.center.x, end.yMin, 0);

            Vector3 endTan = endPos + Vector3.down*50;
            Color shadowCol = new Color(0, 0, 0, 0.06f);
            for (int i = 0; i < 3; i++) // Draw a shadow
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, 2);
            Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 2);
            Handles.DrawAAConvexPolygon(endPos, endPos + new Vector3(-5, -5, 0), endPos + new Vector3(5, -5, 0));
        }

        private static GUIStyle _selectedNodeStyle;
        private static GUIStyle _deselectedNodeStyle;

        public static GUIStyle DeselectedNodeStyle
        {
            get { return GetNodeStyle(NodeType.Default, false); }
        }

        public static GUIStyle SelectedNodeStyle
        {
            get { return GetNodeStyle(NodeType.Default, true); }
        }

        public static void AddMenuItem(this GenericMenu menu, GUIContent content, bool on, GenericMenu.MenuFunction func)
        {
            if (on)
            {
                menu.AddItem(content, false, func);
            }
            else
            {
                menu.AddDisabledItem(content);
            }
        }

        private static readonly Dictionary<string, GUIStyle> styleCache = new Dictionary<string, GUIStyle>();

        public static GUIStyle GetNodeStyle(NodeType nodeType, bool on)
        {
            string key = string.Format("flow node {0}{1}", (int) nodeType, !on ? (object) string.Empty : (object) " on");
            if (!styleCache.ContainsKey(key))
            {
                styleCache[key] = new GUIStyle(key);
                styleCache[key].padding = new RectOffset(0, 0, 0, 0);
            }
            return styleCache[key];
        }

        public static Node GetInitialNode(List<Node> nodes)
        {
            var entryNode = nodes.FirstOrDefault(x => x.IsEntry);
            if (entryNode == null) return null;
            var entryTransition = entryNode.Transitions.FirstOrDefault();
            if (entryTransition == null) return null;
            return nodes.FirstOrDefault(x => x.Name == entryTransition.TargetState);
        }

        public enum NodeType
        {
            Default = 0,
            Any = 2,
            Entry = 3,
            InitialState = 5
        }

        public static string MakeCodeIdentifier(string input)
        {
            var result = input.RemoveSpecialSymbols();
            var resultArray = result.ToCharArray().ToList();
            if (resultArray.Count > 0 && Char.IsDigit(resultArray[0]))
            {
                resultArray.Insert(0, '_');
            }
            return new string(resultArray.ToArray());
        }

        public static string RemoveSpecialSymbols(this string input)
        {
            if (input == null) return string.Empty;

            return new string(input.ToCharArray()
                .Where(c => char.IsLetterOrDigit(c) || c == '_')
                .ToArray());
        }
        
        public static Rect GetNodesRect(List<Node> nodes)
        {
            if (nodes == null || nodes.Count == 0) return new Rect(0,0,1,1);

            var xMin = nodes[0].Rect.xMin;
            for (var i = 0; i < nodes.Count; i++)
            {
                if (xMin > nodes[i].Rect.xMin)
                {
                    xMin = nodes[i].Rect.xMin;
                }
            }

            var yMin = nodes[0].Rect.yMin;
            for (var i = 0; i < nodes.Count; i++)
            {
                if (yMin > nodes[i].Rect.yMin)
                {
                    yMin = nodes[i].Rect.yMin;
                }
            }
                
            var xMax = nodes[0].Rect.xMax;
            for (var i = 0; i < nodes.Count; i++)
            {
                if (xMax < nodes[i].Rect.xMax)
                {
                    xMax = nodes[i].Rect.xMax;
                }
            }

            var yMax = nodes[0].Rect.yMax;
            for (var i = 0; i < nodes.Count; i++)
            {
                if (yMax < nodes[i].Rect.yMax)
                {
                    yMax = nodes[i].Rect.yMax;
                }
            }
            return new Rect(xMin, yMin, xMax-xMin, yMax-yMin);
        }
    }
}