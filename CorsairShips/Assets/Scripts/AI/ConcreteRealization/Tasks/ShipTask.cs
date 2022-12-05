using AI;

namespace Game.SeaGameplay.AI.Tasks {
    public abstract class ShipTask : UpdatedTask {
        protected Ship Ship { get; private set; }
        protected ShipMovementController MovementController { get; private set; }
        protected ShipAIController ShipAIController { get; private set; }

        public override void Init() {
            base.Init();
            ShipAIController = BehaviourTree.Executor as ShipAIController;
            Ship = ShipAIController?.Ship;
            MovementController = Ship?.MovementController;
        }
    }
}