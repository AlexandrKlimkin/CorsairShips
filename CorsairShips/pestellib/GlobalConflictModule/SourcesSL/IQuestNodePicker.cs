using ServerShared.GlobalConflict;

namespace PestelLib.SharedLogic.Modules
{
    public interface IQuestNodePicker
    {
        /// <summary>
        /// Выбирает ноду в которую будет установлен локальный квест
        /// </summary>
        /// <param name="conflictState"></param>
        /// <param name="team"></param>
        /// <param name="currentQuests"></param>
        /// <returns></returns>
        NodeState PickNode(GlobalConflictState conflictState, string team, GlobalConflictDeployedQuest[] currentQuests);
    }
}