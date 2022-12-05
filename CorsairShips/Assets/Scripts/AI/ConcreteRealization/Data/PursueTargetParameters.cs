using System;
using Sirenix.OdinInspector;
using Tools.BehaviourTree;

namespace Game.SeaGameplay.AI.Data {
    [Serializable]
    public class PursueTargetParameters : IBlackboardData {
        public float AroundDistance;
        public float AroundAngle;
        public bool RandomClockWise;
        public bool ClockWise;
    }
}