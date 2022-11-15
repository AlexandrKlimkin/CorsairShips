using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServerShared.GlobalConflict;

namespace ServerShared.GlobalConflict
{
    public class ValidateQuests : IGlobalConflictValidator
    {
        public bool IsValid(GlobalConflictState state, ValidatorMessageCollection messages)
        {
            foreach (var q in state.Quests.Where(_ => string.IsNullOrEmpty(_.Id)))
            {
                messages.Add(MessageLevel.Error, "Quest has no name.");
            }

            var groups = state.Quests.GroupBy(_ => _.QuestLevel).ToArray();
            var weightsByLevel = groups.ToDictionary(_ => _.Key, _ => _.Sum(t => t.Weight));
            foreach (var kv in weightsByLevel.Where(_=>_.Value == 0))
            {
                messages.Add(MessageLevel.Error, string.Format("Total weight of all quests t lvl {0} is 0.", kv.Key));
            }

            if (state.Quests.Length > 0)
            {
                var qMinLvl = state.Quests.Min(_ => _.QuestLevel);
                if (qMinLvl != 0)
                {
                    messages.Add(MessageLevel.Error, string.Format("Quest level 0 must be minimum level"));
                }
                var qMaxLvl = state.Quests.Max(_ => _.QuestLevel);
                foreach (var missedQlvl in Enumerable.Range(qMinLvl, qMaxLvl - qMinLvl)
                    .Where(_ => state.Quests.All(q => q.QuestLevel != _)))
                {
                    messages.Add(MessageLevel.Error, string.Format("Quest level {0} is missed in the sequence.", missedQlvl));
                }
            }

            foreach (var nodeQuest in state.Quests)
            {
                if (string.IsNullOrEmpty(nodeQuest.ClientType))
                {
                    messages.Add(MessageLevel.Error, string.Format("Quest {0}. Set quest Type.", nodeQuest.Id));
                }

                if (nodeQuest.ActiveTime <= TimeSpan.Zero)
                {
                    messages.Add(MessageLevel.Error, string.Format("Quest {0} active time can't be zero", nodeQuest.Id));
                }

                if (nodeQuest.DeployCooldown <= TimeSpan.Zero)
                {
                    messages.Add(MessageLevel.Error, string.Format("Quest {0} de[loy cooldown can't be zero", nodeQuest.Id));
                }
            }

            return messages.Errors == 0;
        }
    }
}
