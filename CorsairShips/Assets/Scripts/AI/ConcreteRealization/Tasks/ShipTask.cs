using AI;

namespace Game.SeaGameplay.AI.Tasks {
    public abstract class ShipTask : UpdatedTask {
        protected Ship Ship { get; private set; }
        protected ShipMovementController MovementController { get; private set; }
        protected BotAIController BotAIController { get; private set; }

        public override void Init() {
            base.Init();
            BotAIController = BehaviourTree.Executor as BotAIController;
            Ship = BotAIController?.Ship;
            MovementController = Ship?.MovementController;
        }
    }
}