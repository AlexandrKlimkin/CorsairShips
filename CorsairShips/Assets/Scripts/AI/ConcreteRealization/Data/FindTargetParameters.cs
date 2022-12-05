using System;
using Tools.BehaviourTree;
using UnityEngine;

namespace Game.SeaGameplay.AI.Data {
    [Serializable]
    public class FindTargetParameters : IBlackboardData {
        public float Radius;
        [Tooltip("Period if unit has no target")]
        public float FindTargetPeriod;
        [Tooltip("Period if unit has target")]
        public float RefreshTargetPeriod;
        public float TryFindMissedTargetTime;
        [Header("Weights")]
        public float DistanceWeight = 1f;
        // public float AttitudeWeight = 0.1f;
        // public bool AggroIfTakeDamage = true;
    }
}