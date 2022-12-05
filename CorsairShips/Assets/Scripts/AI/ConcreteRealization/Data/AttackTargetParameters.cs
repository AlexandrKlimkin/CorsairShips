using System;
using Tools.BehaviourTree;

namespace Game.SeaGameplay.AI.Data {
    [Serializable]
    public class AttackTargetParameters : IBlackboardData {
        public float CheckCD;
        public float ValidDotToTarget;
        public float MaxDistance;
    }
}