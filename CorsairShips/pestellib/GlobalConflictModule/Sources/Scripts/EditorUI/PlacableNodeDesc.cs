using System;
using System.Collections.Generic;
using System.Linq;
using ServerShared.GlobalConflict;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GlobalConflictModule.Scripts
{
    [ExecuteInEditMode]
    public class PlacableNodeDesc : MonoBehaviour
    {
        private List<PlacableNodeDesc> _linkedNodes = new List<PlacableNodeDesc>();

        private bool _inited;

        public NodeState NodeState = new NodeState() {LinkedNodes = new int[] { }};
        public int NodeId;
        public Action<Vector2> NodePlaced = pos => { Debug.Log("Node placed at " + pos); };

        void OnDestroy()
        {
            if (!_inited)
                return;

            Selection.selectionChanged -= SelectionChanged;
        }

        void UpdateEditor()
        {
            NodeState.Id = NodeId;
            this.name = "Node#" + NodeId;
            var conflictEditor = GetComponentInParent<GlobalConflictDesc>();
            if (conflictEditor.ListsDestroyed)
                return;
            if (_linkedNodes != null)
            {
                if (_linkedNodes.Count != NodeState.LinkedNodes.Length)
                {
                    _linkedNodes.Clear();
                    foreach (var nId in NodeState.LinkedNodes)
                    {
                        var n = conflictEditor.FindNode(nId);
                        if (n == null)
                            continue;
                        _linkedNodes.Add(n);
                    }
                }

                foreach (var placableNode in _linkedNodes)
                {
                    if (placableNode == null)
                        continue;

                    if (placableNode._linkedNodes.All(_ => _.NodeId != NodeId))
                    {
                        placableNode._linkedNodes.Add(this);
                    }

                    if (!placableNode.NodeState.LinkedNodes.Contains(NodeId))
                    {
                        placableNode.NodeState.LinkedNodes =
                            placableNode.NodeState.LinkedNodes.Union(new[] { NodeId }).ToArray();
                    }

                    if (placableNode._linkedNodes.Count != placableNode.NodeState.LinkedNodes.Length)
                    {
                        var list = placableNode.NodeState.LinkedNodes.ToList();
                        list.RemoveAll(_ => placableNode._linkedNodes.All(l => l.NodeId != _));
                        placableNode.NodeState.LinkedNodes = list.ToArray();
                    }
                }
            }

            var pos = GetRelativePosition();
            NodeState.PositionX = pos.x;
            NodeState.PositionY = pos.y;
        }

        void Update()
        {
            if (!_inited)
            {
                Selection.selectionChanged += SelectionChanged;
                _inited = true;
            }

            if (!Application.isPlaying)
                UpdateEditor();
        }

        private void SelectionChanged()
        {
            var highlight = Selection.objects.OfType<GameObject>().Select(_ => _.GetComponent<PlacableNodeDesc>())
                .Where(_ => _ != null).Any(_ => _.NodeId == NodeId);
            var img = GetComponent<Image>();
            img.color = highlight ? Color.red : Color.white;
        }

        void OnGUI()
        {
        }

        public static Vector2 GetApectSize(RectTransform container, Image map)
        {
            var mapAspect = map.preferredWidth / map.preferredHeight;
            var containerAspect = container.rect.width / container.rect.height;
            var sz = containerAspect > mapAspect ? new Vector2(map.preferredWidth * container.rect.height / map.preferredHeight, container.rect.height) : new Vector2(container.rect.width, map.preferredHeight * container.rect.width / map.preferredWidth);
            return sz;
        }

        public Vector2 GetRelativePosition()
        {
            var parent = transform.parent.gameObject.GetComponent<RectTransform>();
            var parentImage = transform.parent.gameObject.GetComponent<Image>();
            var hSz = GetApectSize(parent, parentImage) / 2;
            var origin = new Vector2(parent.transform.position.x, parent.transform.position.y);
            var nodePos = new Vector2(transform.position.x, transform.position.y) - origin;
            var nodeRel = new Vector2(nodePos.x / hSz.x, nodePos.y / hSz.y);
            return nodeRel;
        }

        public void UpdatePosition()
        {
            var parent = transform.parent.gameObject.GetComponent<RectTransform>();
            var hSz = parent.rect.size / 2;
            var origin = new Vector2(parent.transform.position.x, parent.transform.position.y);
            var p = origin + new Vector2(NodeState.PositionX * hSz.x, NodeState.PositionY * hSz.y);
            transform.position = p;
        }

        public void AddLink(PlacableNodeDesc node)
        {
            if (node.NodeId == NodeId)
                return;
            
            if (NodeState.LinkedNodes.Any(_ => _ == node.NodeId))
                return;

            NodeState.LinkedNodes = NodeState.LinkedNodes.Union(new[] {node.NodeId}).ToArray();
            _linkedNodes.Add(node);
        }

        public void RemoveLink(PlacableNodeDesc node)
        {
            if (node.NodeId == NodeId)
                return;
            _linkedNodes.RemoveAll(_ => _.NodeId == node.NodeId);
            var list = NodeState.LinkedNodes.ToList();
            list.RemoveAll(_ => _ == node.NodeId);
            NodeState.LinkedNodes = list.ToArray();
        }

        public void OnDrawGizmos()
        {
            if (_linkedNodes == null)
                return;
            foreach (var node in _linkedNodes)
            {
                if (node == null)
                    continue;
                Gizmos.DrawLine(transform.position, node.transform.position);
            }
        }
    }
}