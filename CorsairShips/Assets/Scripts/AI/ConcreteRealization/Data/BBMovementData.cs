using Tools.BehaviourTree;
using UnityEngine;

namespace Game.SeaGameplay.AI.Data {
    public class BBMovementData : IBlackboardData {
        public Vector3? TargetPoint;
        public MovementType? MovementType;
        public float StopDistance;
        public bool DestinationReached;
    }
	
    public enum MovementType { RandomPoint, Retreat, Pursuit, KeepDistance, ConcretePoint, PathPoints }
}