using Game.SeaGameplay.AI.Data;
using Tools.BehaviourTree;
using UnityEngine;

namespace Game.SeaGameplay.AI.Tasks {
    public class AttackTargetTask : ShipTask {

        private AttackTargetParameters _AttackParams;
        private BBTargetData _TargetData;
        
        
        public override void Init() {
            base.Init();
            _AttackParams = Blackboard.Get<AttackTargetParameters>();
            _TargetData = Blackboard.Get<BBTargetData>();
        }

        public override TaskStatus Run() {
            if (_TargetData.Target == null)
                return TaskStatus.Failure;
            if (Ship.WeaponController.NormilizedCD < 1)
                return TaskStatus.Failure;
            var vectorToTarget = _TargetData.Target.Position - Ship.Position;
            var dirToTarget = vectorToTarget.normalized;

            var dot = Vector3.Dot(dirToTarget, Ship.transform.forward);
            if (dot > _AttackParams.ValidDotToTarget)
                return TaskStatus.Failure;

            var sqrDistToTarget = vectorToTarget.sqrMagnitude;
            if (sqrDistToTarget > _AttackParams.MaxDistance * _AttackParams.MaxDistance)
                return TaskStatus.Failure;

            Ship.WeaponController.TryFire();
            
            return TaskStatus.Success;
        }
    }
}