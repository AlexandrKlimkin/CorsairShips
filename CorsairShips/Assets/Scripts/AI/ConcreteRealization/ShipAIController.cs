using UnityDI;
using Tools.BehaviourTree;
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
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            Ship.OnDie -= OnShipDie;
            _AIService?.Unregister(this);
        }
    }
}