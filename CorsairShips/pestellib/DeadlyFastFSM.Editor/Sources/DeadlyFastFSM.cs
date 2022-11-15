using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using PestelLib.DeadlyFastFSM.Editor;
using UnityEditor.Graphs;

namespace DeadlyFast
{
    public class DeadlyFastFSM : EditorWindow
    {
        private static readonly int kDragGraphControlID = "DragGraph".GetHashCode();

        private Color _statusColor = Color.white;
        private string _status;
        // The position of the window

        private bool IsWaitingForTargetSelection = false;
        private Node CurrentNode;
        private Transition CurrentTransition;
        private int CurrentTransitionIndex;
        private bool _wasFocused = false;

        private FsmData _data;

        public List<Node> nodes
        {
            get { return _data != null ? _data.Nodes : null; }
        }

        // Scroll position
        private Vector2 scrollPos = new Vector2(0, 0);

        private bool _mouseDown;

        private Rect m_LastGraphExtents;
        private Rect graphExtents;

        private Vector2 _min;
        private Vector2 _max;

        //целиком из меканима- процедура отрисовки фона

        //целиком из меканима - перетаскивание всей рабочей области
        private void DragGraph()
        {
            int controlId = GUIUtility.GetControlID(kDragGraphControlID, FocusType.Passive);
            Event current = Event.current;
            if (current.button != 2 && (current.button != 0 || !current.alt))
                return;
            switch (current.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    GUIUtility.hotControl = controlId;
                    current.Use();
                    EditorGUIUtility.SetWantsMouseJumping(1);
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl != controlId)
                        break;
                    GUIUtility.hotControl = 0;
                    current.Use();
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                case EventType.MouseMove:
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl != controlId)
                        break;
                    scrollPos -= current.delta;
                    current.Use();
                    break;
            }
        }

        //целиком из меканима - отрисовка линий сетки

        //из меканима - отрисовка одной линии

        void OnSelectionChange()
        {
            var dataBefore = _data;
            TryUpdateSelectedObject();
            Repaint();
            if (dataBefore != _data)
            {
                _wasFocused = false;
                Repaint();
            }
        }

        public void Update()
        {
            if (IsWaitingForTargetSelection)
            {
                Repaint();
            }
        }

        private Vector2 ToScrollerSpace(Vector2 windowSpace)
        {
            return windowSpace + scrollPos + Min;
        }

        private Vector2 ToWindowSpace(Vector2 scrollerSpace)
        {
            return scrollerSpace - scrollPos - Min;
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed")
            {
                Repaint();
            }

            if (Event.current.type == EventType.MouseDown)
            {
                _status = string.Empty;
            }

            TryUpdateSelectedObject();

            if (nodes == null)
            {
                ShowMessage("Select existing 'Deadly Fast FSM' or create new one with 'Assets/Create/Deadly Fast FSM'");
                DrawStatusline();
                return;
            }

            if (nodes.Count == 2)
            {
                ShowMessage("Use right mouse button to create new state");
            }

            if (Event.current.keyCode == KeyCode.F5)
            {
                FocusOnCenter();
            }

            if (Event.current.type == EventType.Repaint)
                Styles.graphBackground.Draw(
                    new Rect(0, 0, position.width, position.height),
                    false,
                    false,
                    false,
                    false
                    );


            CropEmptySpace();

            graphExtents = new Rect(
                Min.x,
                Min.y,
                Max.x - Min.x,
                Max.y - Min.y
                );

            scrollPos = GUI.BeginScrollView(
                new Rect(0, 0, this.position.width, this.position.height),
                scrollPos,
                graphExtents, GUIStyle.none, GUIStyle.none
                );

            GridDrawer.DrawGrid(Min, Max);

            BeginWindows();

            var initialNode = Utils.GetInitialNode(nodes);

            for (var i = 0; i < nodes.Count; i++)
            {
                nodes[i].Rect.width = 10; //если не сбрасывать каждый раз размеры, они не адаптируются к содержимому, например после удаления линка
                nodes[i].Rect.height = 10;

                var isNodeSelected = selection.Contains(nodes[i]);
                var isFirstNode = initialNode == nodes[i];

                var resultingRect = GUILayout.Window(i, nodes[i].Rect, DoWindow, "", nodes[i].GetStyle(isNodeSelected, isFirstNode));
                nodes[i].Rect.size = resultingRect.size; //но после отрисовки нам нужны правильные размеры Rect у каждой ноды. Но не нужны позиции - т.к. они меняются через Drag в DoWindow
            }

            EndWindows();
            DrawLinks();
            TryDrawCurrentLink();
            DragGraph();
            UpdateScrollPosition();
            GUI.EndScrollView();
            ShowEmptyAreaContextMenu();
            TryStopRenaming();
            DragSelection();
            DrawStatusline();
            if (!_wasFocused)
            {
                _wasFocused = true;
                FocusOnCenter();
            }
        }

        private void DrawStatusline()
        {
            GUI.contentColor = _statusColor;
            EditorGUILayout.LabelField(_status);
        }

        private void FocusOnCenter()
        {
            var nodesRect = Utils.GetNodesRect(nodes);
            var nodesCenter = ToWindowSpace(nodesRect.center);
            var offsetFromCenter = nodesCenter - (position.size*0.5f);
            scrollPos = scrollPos + offsetFromCenter;
            Repaint();
        }

        private void TryDrawCurrentLink()
        {
            if (IsWaitingForTargetSelection)
            {
                var emptyAreaClick = Event.current.type == EventType.MouseDown && 
                                     Event.current.button == 0;

                var escPressed = Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Escape;

                if (emptyAreaClick || escPressed)
                {
                    IsWaitingForTargetSelection = false; //cancel link by click on empty area

                    if (!CurrentNode.IsEntry)
                    {
                        if (string.IsNullOrEmpty(CurrentTransition.TargetState))
                        {
                            Undo.RecordObject(_data, "Removing transition");
                            CurrentNode.Transitions.RemoveAt(CurrentTransitionIndex);
                            MarkForSave();
                            Undo.FlushUndoRecordObjects();
                        }
                    }
                    Repaint();
                    return;
                }

                var targetRect = new Rect(Event.current.mousePosition, Vector2.one);

                Utils.DrawNodeCurve(CurrentNode.Rect, targetRect, CurrentTransitionIndex, Color.white);
            }
        }

        private Color GetColor(int nodeIndex, int transitionIndex)
        {
            var hash = nodeIndex ^ transitionIndex; 
            var rand = new System.Random(hash);
            var randomValue = (float)rand.NextDouble();
            return Color.HSVToRGB(randomValue, 1, 1);
        }

        private void TryStopRenaming()
        {
            if (Event.current.type == EventType.MouseDown)
            {
                StopRenaming();
            }
        }

        private void StopRenaming()
        {
            foreach (var node in nodes)
            {
                if (node.RenameInProcess)
                {
                    node.RenameInProcess = false;
                    if (node.NameCandidate == Utils.MakeCodeIdentifier(node.Name)) continue;

                    if (string.IsNullOrEmpty(node.NameCandidate))
                    {
                        ShowError("State name must contain at least one symbol");
                        node.NameCandidate = node.Name;
                        continue;
                    }
                    if (!IsNodeNameFree(node.NameCandidate))
                    {
                        ShowError("State name must be unique");
                        node.NameCandidate = node.Name;
                        continue;
                    }
                    RenameNode(node.Name, Utils.MakeCodeIdentifier(node.NameCandidate));
                    MarkForSave();
                }

                foreach (var transition in node.Transitions)
                {
                    if (transition.RenameInProcess)
                    {
                        transition.RenameInProcess = false;
                        var newEventName = Utils.MakeCodeIdentifier(transition.EventCandidate);
                        if (transition.Event != newEventName)
                        {
                            if (node.Transitions.All(x => x.Event != newEventName))
                            {
                                Undo.RecordObject(_data, "Rename transition event");
                                transition.Event = newEventName;
                                Undo.FlushUndoRecordObjects();
                            }
                            else
                            {
                                ShowError("Transition with event '" + newEventName +
                                                 "' already exist, please use another one.");
                            }
                        }

                        if (string.IsNullOrEmpty(transition.Event))
                        {
                            ShowError("Can't use empty string for event. Auto assigning unique event name.");
                            transition.Event = GetUniqueTransitionEvent(node);
                        }

                        MarkForSave();
                    }
                }
            }
        }

        private void DrawLinks()
        {
            for (var nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
            {
                var node = nodes[nodeIndex];

                for (var transitionIndex = 0; transitionIndex < node.Transitions.Count; transitionIndex++)
                {
                    var transition = node.Transitions[transitionIndex];
                    var target = _data.NodeByName(transition.TargetState);
                    if (target != null)
                    {
                        var color = GetColor(nodeIndex, transitionIndex);
                        Utils.DrawNodeCurve(node.Rect, target.Rect, transitionIndex, color);
                    }
                }
            }
        }

        private void TryUpdateSelectedObject()
        {
            if (Selection.activeObject is FsmData)
            {
                _data = (FsmData) Selection.activeObject;
                if (_data.Nodes == null)
                {
                    _data.Nodes = new List<Node>();
                }
                if (!_data.Nodes.Any(x => x.IsEntry))
                {
                    var pos = Vector2.right * 100f;
                    _data.Nodes.Add(
                        new Node
                        {
                            Name = "Entry",
                            Position = pos,
                            Rect = new Rect(pos, Vector2.one),
                            Transitions = new List<Transition>
                            {
                                new Transition
                                {
                                    Event = "Start",
                                    EventCandidate = "Start"
                                }
                            }
                        }
                    );
                }

                if (!_data.Nodes.Any(x => x.IsAnyState))
                {
                    var pos = Vector2.right*300f;
                    _data.Nodes.Add(
                        new Node
                        {
                            Name = "AnyState",
                            Transitions = new List<Transition>(),
                            Position = pos,
                            Rect = new Rect(pos, Vector2.one)
                        }
                    );
                }
            }
        }

        private void CropEmptySpace()
        {
            if (Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseUp ||
                Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDown)
            {
                //left
                {
                    var distToEnd = RealMin.x - Min.x;
                    var distToScrollBorder = distToEnd - scrollPos.x;
                    var emptySpace = (distToEnd - distToScrollBorder);
                    _min.x += emptySpace;
                }
                //top
                {
                    var distToEnd = RealMin.y - Min.y;
                    var distToScrollBorder = distToEnd - scrollPos.y;
                    var emptySpace = (distToEnd - distToScrollBorder);
                    _min.y += emptySpace;
                }
                //right 
                {
                    var distToEnd = Max.x - RealMax.x;
                    var diff = (graphExtents.width - this.position.width);
                    var offsetFromEnd = scrollPos.x + diff;
                    var distToScrollBorder = distToEnd - offsetFromEnd;
                    var emptySpace = (distToEnd - distToScrollBorder);
                    _max.x -= emptySpace;
                }
                //bottom
                {
                    var distToEnd = Max.y - RealMax.y;
                    var diff = (graphExtents.height - this.position.height);
                    var offsetFromEnd = scrollPos.y + diff;
                    var distToScrollBorder = distToEnd - offsetFromEnd;
                    var emptySpace = (distToEnd - distToScrollBorder);
                    _max.y -= emptySpace;
                }

                Repaint();
            }
        }

        private void UpdateScrollPosition()
        {
            scrollPos.x += this.m_LastGraphExtents.x - this.graphExtents.x;
            scrollPos.y += this.m_LastGraphExtents.y - this.graphExtents.y;
            this.m_LastGraphExtents = this.graphExtents;
        }

        private Vector2 RealMin
        {
            get
            {
                var nodesRect = Utils.GetNodesRect(nodes);
                return new Vector2(nodesRect.x - position.width, nodesRect.y - position.height);
            }
        }

        private Vector2 Min
        {
            get
            {
                var realMin = RealMin;

                if (_min.x > realMin.x)
                {
                    _min.x = realMin.x;
                }
                if (_min.y > realMin.y)
                {
                    _min.y = realMin.y;
                }

                return _min; //new Vector2(xMin, yMin);
            }
        }

        private Vector2 RealMax
        {
            get
            {
                var nodesRect = Utils.GetNodesRect(nodes);
                return new Vector2(nodesRect.xMax + position.width, nodesRect.yMax + position.height);
            }
        }

        private Vector2 Max
        {
            get
            {
                var realMax = RealMax;

                if (_max.x < realMax.x)
                {
                    _max.x = realMax.x;
                }
                if (_max.y < realMax.y)
                {
                    _max.y = realMax.y;
                }

                return _max;
            }
        }

        private void DoWindow(int nodeIndex)
        {
            var node = nodes[nodeIndex];

            if (Event.current.type == EventType.KeyDown && 
                (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
            {
                StopRenaming();
                Event.current.Use();
            }

            if (Event.current.type == EventType.MouseDown && !node.RenameInProcess && !node.Transitions.Any(x => x.RenameInProcess))
            {
                StopRenaming();
            }

            if (Event.current.type == EventType.MouseDown && IsWaitingForTargetSelection)
            {
                if (node.IsEntry)
                {
                    ShowError("Can't set 'Entry' as target state! Please select another one.");
                } else if (node.IsAnyState)
                {
                    ShowError("Can't set 'AnyState' as target state!");
                }
                else
                {
                    Undo.RecordObject(_data, "Set target state for transition");
                    CurrentTransition.TargetState = node.Name;
                    if (string.IsNullOrEmpty(CurrentTransition.Event))
                    {
                        CurrentTransition.RenameInProcess = true;
                    }
                    IsWaitingForTargetSelection = false;
                    MarkForSave();
                    Undo.FlushUndoRecordObjects();
                }
                Event.current.Use();
            }
            
            /*
            if (Event.current.type == EventType.mouseDown && Event.current.clickCount == 2)
            {
                StartRenamingNode(node);
                Event.current.Use();
            }*/

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete && !node.RenameInProcess && !node.Transitions.Any(x => x.RenameInProcess))
            {
                RemoveNode(nodeIndex);
            }

            if (focusedWindow != this)
            {
                StopRenaming();
            }

            if (node.RenameInProcess)
            {
                var fieldName = "RenameState" + nodeIndex;
                GUI.SetNextControlName(fieldName);
                node.NameCandidate = GUILayout.TextField(node.NameCandidate);
                GUI.FocusControl(fieldName);
            }
            else
            {
                GUILayout.Label(node.Name);
            }
            
            for (var transitionIndex = 0; transitionIndex < node.Transitions.Count; transitionIndex++)
            {
                var transition = node.Transitions[transitionIndex];

                if (transition.RenameInProcess)
                {
                    var fieldName = "RenameTransition" + nodeIndex + "_" + transitionIndex;
                    GUI.SetNextControlName(fieldName);
                    transition.EventCandidate = GUILayout.TextField(transition.EventCandidate);
                    GUI.FocusControl(fieldName);
                }
                else
                {
                    if (GUILayout.Button(transition.Event))
                    {
                        if (Event.current.button == 1) //RMB
                        {
                            ShowTransitionContextMenu(node, transitionIndex);
                        }
                        else if (Event.current.button == 0) //LMB
                        {
                            SetNewTargetStateForTransition(node, transitionIndex);
                        }
                    }
                }
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive);
                if (!selection.Contains(node))
                {
                    ClearSelection();
                }
                selection.Add(node);
                Event.current.Use();
            }

            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                GUIUtility.hotControl = 0;
                Event.current.Use();
            }

            if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
            {
                Undo.RecordObject(_data, "Moving states");
                foreach (var n in selection)
                {
                    if (n != node)
                    {
                        n.Rect = new Rect(n.Rect.position + Event.current.delta, n.Rect.size);
                    }
                }
                node.Rect = new Rect(node.Rect.position + Event.current.delta, node.Rect.size);
                Event.current.Use();
            }

            ShowNodeContextMenu(nodeIndex);
        }

        private void RemoveNode(int nodeIndex)
        {
            Undo.RecordObject(_data, "Removing state");
            var removedNodeName = nodes[nodeIndex].Name;
            nodes.RemoveAt(nodeIndex);
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                for (var j = node.Transitions.Count - 1; j >= 0; j--)
                {
                    if (node.Transitions[j].TargetState == removedNodeName)
                    {
                        if (node.IsEntry)
                        {
                            node.Transitions[j].TargetState = string.Empty;
                        }
                        else
                        {
                            node.Transitions.RemoveAt(j);
                        }
                    }
                }
            }
            MarkForSave();
            Repaint();
            Undo.FlushUndoRecordObjects();
        }

        private void MarkForSave()
        {
            EditorUtility.SetDirty(_data);
        }

        private void ShowTransitionContextMenu(Node node, int transitionIndex)
        {
            Event.current.Use();

            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Set target state"), false, () =>
            {
                SetNewTargetStateForTransition(node, transitionIndex);
            });
            if (!node.IsEntry)
            {
                genericMenu.AddItem(new GUIContent("Delete transition"), false, () =>
                {
                    Undo.RecordObject(_data, "Delete transition");
                    node.Transitions.RemoveAt(transitionIndex);
                    MarkForSave();
                    Undo.FlushUndoRecordObjects();
                });
                genericMenu.AddItem(new GUIContent("Rename event"), false, () =>
                {
                    node.Transitions[transitionIndex].RenameInProcess = true;
                    node.Transitions[transitionIndex].EventCandidate = node.Transitions[transitionIndex].Event;
                });
            }
            else
            {
                genericMenu.AddDisabledItem(new GUIContent("Delete transition"));
                genericMenu.AddDisabledItem(new GUIContent("Rename event"));
            }

            genericMenu.ShowAsContext();
        }

        private void RenameNode(string oldName, string newName)
        {
            if (oldName == newName) return;

            Undo.RecordObject(_data, "Rename state");
            foreach (var node in _data.Nodes)
            {
                if (node.Name == oldName)
                {
                    node.Name = newName;
                }
                foreach (var transition in node.Transitions)
                {
                    if (transition.TargetState == oldName)
                    {
                        transition.TargetState = newName;
                    }
                }
            }
            Undo.FlushUndoRecordObjects();
        }

        private bool IsNodeNameFree(string nodeName)
        {
            nodeName = Utils.MakeCodeIdentifier(nodeName);
            return !_data.Nodes.Any(x => x.Name == nodeName);
        }

        private void StartRenamingNode(Node data)
        {
            StopRenaming();
            data.NameCandidate = data.Name;
            data.RenameInProcess = true;
        }

        private void ShowNodeContextMenu(int nodeIndex)
        {
            if (Event.current.type != EventType.MouseDown || Event.current.button != 1 || Event.current.clickCount != 1)
                return;
            Event.current.Use();

            var node = nodes[nodeIndex];

            GenericMenu genericMenu = new GenericMenu();

            genericMenu.AddMenuItem(new GUIContent("Delete state"), 
                !node.IsEntry && !node.IsAnyState, 
                () => {
                    RemoveNode(nodeIndex);
                }
            );

            genericMenu.AddMenuItem(new GUIContent("Rename state"), 
                !node.IsEntry && !node.IsAnyState, 
                () => {
                    StartRenamingNode(nodes[nodeIndex]);
                }
            );

            genericMenu.AddMenuItem(new GUIContent("Add transition"), 
                !node.IsEntry, 
                () => {
                    AddTransition(nodeIndex);
                    MarkForSave();
                }
            );

            genericMenu.ShowAsContext();
        }

        private void AddTransition(int nodeIndex)
        {
            var transition = new Transition {Event = "", EventCandidate = ""};
            nodes[nodeIndex].Transitions.Add(transition);
            SetNewTargetStateForTransition(nodes[nodeIndex], nodes[nodeIndex].Transitions.Count - 1);
        }

        private void SetNewTargetStateForTransition(Node node, int transitionIndex)
        {
            CurrentNode = node;
            CurrentTransition = node.Transitions[transitionIndex];
            CurrentTransitionIndex = transitionIndex;
            IsWaitingForTargetSelection = true;
            ShowMessage("Select target state");
        }

        private string GetUniqueStateName()
        {
            for (var i = 1; i <= _data.Nodes.Count; i++)
            {
                var nameCandidate = "State" + i;
                if (_data.Nodes.All(x => x.Name != nameCandidate))
                {
                    return nameCandidate;
                }
            }
            return "UnnamedState";
        }

        private string GetUniqueTransitionEvent(Node node)
        {
            for (var i = 1; i <= node.Transitions.Count; i++)
            {
                var nameCandidate = "UndefinedEvent" + i;
                if (node.Transitions.All(x => x.Event != nameCandidate))
                {
                    return nameCandidate;
                }
            }
            return string.Empty;
        }

        protected void ShowEmptyAreaContextMenu()
        {
            if (Event.current.type != EventType.MouseDown || Event.current.button != 1 || Event.current.clickCount != 1)
                return;
            Event.current.Use();
            Vector2 mousePosition = Event.current.mousePosition;
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Add state"), false, () =>
            {
                var pos = mousePosition + OriginOffset;
                nodes.Add(new Node
                {
                    Name = GetUniqueStateName(),
                    Rect = new Rect(pos.x, pos.y, 10, 10),
                    Transitions = new List<Transition>()
                });
            });
            genericMenu.AddItem(new GUIContent("Generate FSM code"), false, () =>
            {
                var filePath = FsmGenerator.GenerateFSMCode(_data);
                ShowMessage("Code generation result: " + filePath);
            });
            genericMenu.ShowAsContext();
        }

        private Vector2 OriginOffset
        {
            get { return (scrollPos + Min); }
        }

        [MenuItem("Window/Deadly Fast FSM")]
        private static void Init()
        {
            var win = (DeadlyFastFSM)EditorWindow.GetWindow(typeof (DeadlyFastFSM), false, "FSM");
            win.wantsMouseMove = true;
        }

        #region SelectionRectangle
        private static readonly int kDragSelectionControlID = "DragSelection".GetHashCode();
        private Vector2 m_DragStartPoint;
        private SelectionDragMode m_IsDraggingSelection;
        public List<Node> selection = new List<Node>();

        private enum SelectionDragMode
        {
            None,
            Rect,
            Pick,
        }
        internal static Rect FromToRect(Vector2 start, Vector2 end)
        {
            Rect rect = new Rect(start.x, start.y, end.x - start.x, end.y - start.y);
            if ((double)rect.width < 0.0)
            {
                rect.x += rect.width;
                rect.width = -rect.width;
            }
            if ((double)rect.height < 0.0)
            {
                rect.y += rect.height;
                rect.height = -rect.height;
            }
            return rect;
        }

        private void SelectNodesInRect(Rect r)
        {
            this.selection.Clear();
            foreach (var current in nodes)
            {
                Rect rect = current.Rect;
                if ((double)rect.xMax >= (double)r.x && (double)rect.x <= (double)r.xMax && ((double)rect.yMax >= (double)r.y && (double)rect.y <= (double)r.yMax))
                    this.selection.Add(current);
            }
        }

        public virtual void ClearSelection()
        {
            this.selection.Clear();
        }

        protected void DragSelection()
        {
            var position = new Rect(-5000f, -5000f, 10000f, 10000f);

            int controlId = GUIUtility.GetControlID(kDragSelectionControlID, FocusType.Passive);
            Event current = Event.current;
            switch (current.GetTypeForControl(controlId))
            {
                case EventType.MouseDown:
                    if (!position.Contains(current.mousePosition) || current.button != 0 || (current.clickCount == 2 || current.alt))
                        break;
                    GUIUtility.hotControl = controlId;
                    this.m_DragStartPoint = current.mousePosition;
                    current.Use();
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl != controlId)
                        break;
                    GUIUtility.hotControl = 0;
                    if (m_IsDraggingSelection == SelectionDragMode.None)
                    {
                        ClearSelection();
                    }
                    this.m_IsDraggingSelection = SelectionDragMode.None;
                    current.Use();
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl != controlId)
                        break;
                    if (!EditorGUI.actionKey && !current.shift && this.m_IsDraggingSelection == SelectionDragMode.Pick)
                        this.ClearSelection();
                    this.m_IsDraggingSelection = SelectionDragMode.Rect;
                    this.SelectNodesInRect(FromToRect(this.m_DragStartPoint + OriginOffset, current.mousePosition + OriginOffset));
                    current.Use();
                    break;
                case EventType.KeyDown:
                    if (this.m_IsDraggingSelection == SelectionDragMode.None || current.keyCode != KeyCode.Escape)
                        break;
                    this.selection = null;
                    GUIUtility.hotControl = 0;
                    this.m_IsDraggingSelection = SelectionDragMode.None;
                    current.Use();
                    break;
                case EventType.Repaint:
                    if (this.m_IsDraggingSelection != SelectionDragMode.Rect)
                        break;
                    Styles.selectionRect.Draw(FromToRect(this.m_DragStartPoint, current.mousePosition), false, false, false, false);
                    break;
            }
        }
        #endregion

        private void ShowError(string msg)
        {
            _statusColor = Color.red;
            _status = msg;
            Debug.LogWarning(msg);
        }

        private void ShowMessage(string msg)
        {
            _statusColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            _status = msg;
            Debug.Log(msg);
        }
    }
}