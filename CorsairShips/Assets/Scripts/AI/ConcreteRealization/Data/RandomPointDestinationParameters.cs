using System;
using Tools.BehaviourTree;
using UnityEditor;
using UnityEngine;

namespace Game.SeaGameplay.AI.Data {
    [Serializable]
    public class RandomPointDestinationParameters : IBlackboardData {
        public float AreaRadius;
        public Transform AreaCenter;
        public bool ReleaseAreaCenter;
        public Vector2 RandomWaitTime;
        [Range(0, 1)]
        public float WaitAtPointChance;
        [Range(0.1f, 10f)]
        public float StopDistance;

        public void DrawGizmos() {
#if UNITY_EDITOR
            Handles.color = Color.white;
            Handles.DrawWireDisc(AreaCenter.position, Vector3.up, AreaRadius);
#endif
        }
    }
}