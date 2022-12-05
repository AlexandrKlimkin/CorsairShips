using Game.SeaGameplay.AI.Data;
using Tools.BehaviourTree;
using UnityEngine;

namespace Game.SeaGameplay.AI.Tasks {
    public class PursueDestinationTask : ShipTask {
        
        private BBTargetData _TargetData;
        private BBMovementData _MovementData;
        private PursueTargetParameters _PursueParams;
        
        
        public override void Init() {
            base.Init();
            _TargetData = Blackboard.Get<BBTargetData>();
            _MovementData = Blackboard.Get<BBMovementData>();
            _PursueParams = Blackboard.Get<PursueTargetParameters>();

            if (_PursueParams.RandomClockWise)
                _PursueParams.ClockWise = Random.value < 0.5f;
        }
        
        public override TaskStatus Run() {
            if (_TargetData.Target == null)
                return TaskStatus.Failure;
            
            _MovementData.TargetPoint = ShipAIController.GetNearestPositionAroundTarget(_TargetData.Target.Position, _PursueParams.AroundDistance, _PursueParams.AroundAngle, _PursueParams.ClockWise);
            _MovementData.MovementType = MovementType.Pursuit;
            return TaskStatus.Success;
        }
    }
}