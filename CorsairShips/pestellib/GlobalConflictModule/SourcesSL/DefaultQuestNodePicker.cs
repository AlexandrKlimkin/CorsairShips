using System;
using System.Linq;
using PestelLib.SharedLogicBase;
using ServerShared.GlobalConflict;
using ServerShared.Sources.GlobalConflict;

namespace PestelLib.SharedLogic.Modules
{
    class DefaultQuestNodePicker : IQuestNodePicker
    {
        private readonly Func<int, int> _randomProvider;

        public DefaultQuestNodePicker(Func<int, int> randomProvider)
        {
            _randomProvider = randomProvider;
        }

        public NodeState PickNode(GlobalConflictState conflictState, string team, GlobalConflictDeployedQuest[] currentQuests)
        {
            SharedCommandCallstack.CheckCallstack();
            var exclude = currentQuests.Where(_ => !_.Completed).Select(_ => _.NodeId);
            var variants = conflictState.GetReachableNodes(team, true).Select(_ => _.Id).Except(exclude).ToArray();
            if (variants.Length == 0)
                return null;

            var idx = _randomProvider(variants.Length);

            return conflictState.GetNode(variants[idx]);
        }
    }
}