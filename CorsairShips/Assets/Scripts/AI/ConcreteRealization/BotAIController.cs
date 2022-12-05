using Game.SeaGameplay.AI.Data;
using Game.SeaGameplay.AI.Tasks;
using Tools.BehaviourTree;
using UnityEngine;

namespace Game.SeaGameplay.AI {
    public class BotAIController : ShipAIController {

        [SerializeField]
        private FindTargetParameters _FindTargetParameters;
        [SerializeField]
        private AttackTargetParameters _AttackTargetParameters;
        [SerializeField]
        private PursueTargetParameters _PursueTargetParameters;
        [SerializeField]
        private RandomPointDestinationParameters _RandomPointDestinationParameters;
        
        protected override Blackboard BuildBlackboard() {
            var bb = base.BuildBlackboard();
            
            _RandomPointDestinationParameters.AreaCenter.position = Vector3.zero;
            
            bb.Set(_FindTargetParameters);
            bb.Set(_AttackTargetParameters);
            bb.Set(_PursueTargetParameters);
            bb.Set(_RandomPointDestinationParameters);
            return bb;
        }
        
        protected override BehaviourTree BuildBehaviourTree() {
            var bt = new BehaviourTree();
                var main = bt.AddChild<ParallelTask>();
                    var attack = main.AddChild<SequenceTask>();
                        attack.AddChild<FindTargetTask>();
                        attack.AddChild<AttackTargetTask>();
                        
                    var movement = main.AddChild<ParallelTask>();
                        var movementType = movement.AddChild<SelectorTask>();
                            movement.AddChild<PursueDestinationTask>();
                            movementType.AddChild<RandomPointDestinationTask>();
                        movement.AddChild<MoveToTargetPointTask>();
            return bt;
        }
        
        private void OnDrawGizmos() {
            _RandomPointDestinationParameters.DrawGizmos();
        }
    }
}