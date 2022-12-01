using Game.SeaGameplay.AI.Data;
using Game.SeaGameplay.AI.Tasks;
using Tools.BehaviourTree;
using UnityEngine;

namespace Game.SeaGameplay.AI {
    public class BotAIController : ShipAIController {
        
        [SerializeField]
        private RandomPointDestinationParameters _RandomPointDestinationParameters;
        
        protected override Blackboard BuildBlackboard() {
            var bb = base.BuildBlackboard();
            
            _RandomPointDestinationParameters.AreaCenter.position = Vector3.zero;
            
            bb.Set(_RandomPointDestinationParameters);
            return bb;
        }
        
        protected override BehaviourTree BuildBehaviourTree() {
            var bt = new BehaviourTree();
                var movement = bt.AddChild<ParallelTask>();
                    var movementType = movement.AddChild<SelectorTask>();
                        movementType.AddChild<RandomPointDestinationTask>();
                        movement.AddChild<MoveToTargetPointTask>();
            return bt;
        }
        
        private void OnDrawGizmos() {
            _RandomPointDestinationParameters.DrawGizmos();
        }
    }
}