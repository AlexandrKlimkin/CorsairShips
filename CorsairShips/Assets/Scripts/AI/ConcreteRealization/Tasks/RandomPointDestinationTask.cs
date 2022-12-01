using Game.SeaGameplay.AI.Data;
using Tools.BehaviourTree;
using UnityEngine;
using UTPLib.Core.Utils;

namespace Game.SeaGameplay.AI.Tasks {
    public class RandomPointDestinationTask : ShipTask {
        
        private RandomPointDestinationParameters _Parameters;
        private BlackboardMovementData _MovementData;

        private Collider _Collider;
		
        private float _PointReachedTime;
        private float _WaitTime;
        private bool _IsWaiting;
		
        public override void Init() {
            base.Init();
            _Parameters = Blackboard.Get<RandomPointDestinationParameters>();
            _MovementData = Blackboard.Get<BlackboardMovementData>();
			
            if(_Parameters.ReleaseAreaCenter)
                _Parameters.AreaCenter.SetParent(Ship.transform.parent);
			
            IsUpdated = true;
            IsGizmosUpdated = true;
            GetRandomWaitTime();
        }

        public override TaskStatus Run() {
			
            _MovementData.MovementType = MovementType.RandomPoint;
            _MovementData.StopDistance = _Parameters.StopDistance;

            if (_MovementData.DestinationReached && !_IsWaiting) {
                _PointReachedTime = Time.time;
                GetRandomWaitTime();
            }
            if (_MovementData.DestinationReached && Time.time - _PointReachedTime >= _WaitTime) {
                _MovementData.TargetPoint = GetRandomPoint();
                _IsWaiting = false;
            }
            return TaskStatus.Success;
        }

        private Vector3 GetRandomPoint() {
            var randInCircle = Random.insideUnitCircle;
            var point = _Parameters.AreaCenter.position + randInCircle.ToVector3XZ() * _Parameters.AreaRadius;
            return point;
        }

        private void GetRandomWaitTime() {
            var shouldWait = Random.value <= _Parameters.WaitAtPointChance;
            if (shouldWait) {
                _WaitTime = Random.Range(_Parameters.RandomWaitTime.x, _Parameters.RandomWaitTime.y);
                _IsWaiting = true;
            }
            else {
                _WaitTime = 0;
            }
        }
    }
}