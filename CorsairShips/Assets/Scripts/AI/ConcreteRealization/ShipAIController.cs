using UnityDI;
using Tools.BehaviourTree;
using UnityEngine;
using UTPLib.SignalBus;

namespace Game.SeaGameplay.AI {
    public abstract class ShipAIController : BehaviourTreeExecutor {

        [Dependency]
        protected readonly AIService _AIService;

        public Ship Ship { get; private set; }

        protected override void Initialize() {
            ContainerHolder.Container.BuildUp(GetType(), this);
            Ship = GetComponentInParent<Ship>();
            _AIService.Register(this);
            Ship.OnDie += OnShipDie;
        }

        protected override Blackboard BuildBlackboard() {
            return new Blackboard();
        }

        private void OnShipDie(Ship ship) {
            if (ship != Ship)
                return;
            Ship.OnDie -= OnShipDie;
            _AIService.Unregister(this);
            DisposeTasks();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            Ship.OnDie -= OnShipDie;
            _AIService?.Unregister(this);
        }
        
        public Vector3 GetNearestPositionAroundTarget(Vector3 targetPosition, float radius, float angle, bool clockWise = true) {
            var targetDirection = (targetPosition - Ship.Position).normalized;
            var rotationDirection = Mathf.Sign(Vector3.Dot(targetDirection, Ship.transform.right)) * (clockWise ? 1 : -1);
            return targetPosition + Quaternion.AngleAxis(rotationDirection * angle, Vector3.up)
                * targetDirection * radius * -1;
        }
    }
}