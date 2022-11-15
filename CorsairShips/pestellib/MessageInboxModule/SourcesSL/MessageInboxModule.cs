using System.Collections.Generic;
using System.Linq;
using PestelLib.SharedLogicBase;
using ServerShared;

namespace PestelLib.SharedLogic.Modules
{
    public class MessageInboxModule : SharedLogicModule<MessageInboxModuleState>
    {
        public ScheduledAction<long[]> OnMessagesDeleted;

        public MessageInboxModule()
        {
            OnMessagesDeleted = new ScheduledAction<long[]>(ScheduledActionCaller);
        }

        public override void MakeDefaultState()
        {
            State = new MessageInboxModuleState();
            State.ReadMessages = new List<long>();
            State.UnreadMessages = new List<long>();
        }

        [SharedCommand]
        internal long GetEarliestMessage()
        {
            if (State.EarliestStoredMessage == 0)
                State.EarliestStoredMessage = CommandTimestamp.Ticks;

            if (State.StateBirthday == 0)
                State.StateBirthday = State.EarliestStoredMessage;

            if(State.LastSeenMessage == 0)
                State.LastSeenMessage = CommandTimestamp.Ticks;

            return State.EarliestStoredMessage;
        }

        [SharedCommand]
        internal void DeleteAllMessages()
        {
            if (State.ReadMessages.Count < 1)
                return;
            UniversalAssert.IsTrue(State.UnreadMessages.Count == 0, "Partial delete not supported"); 
            if(State.UnreadMessages.Count > 0)
                return;
            var removed = State.ReadMessages.ToArray();
            var max = State.ReadMessages.Max();
            State.ReadMessages.Clear();
            if (max > State.EarliestStoredMessage)
                State.EarliestStoredMessage = max + 1;
            OnMessagesDeleted.Schedule(removed);
        }

        [SharedCommand]
        internal void MarkAsRead(long[] serialIds)
        {
            serialIds = State.UnreadMessages.Where(_ => serialIds.Contains(_)).ToArray();
            State.UnreadMessages.RemoveAll(_ => serialIds.Contains(_));
            State.ReadMessages.AddRange(serialIds);
        }

        [SharedCommand]
        internal void SetWelcomeLetter(long serialId)
        {
            if (State.WelcomeLetter == serialId)
                return;
            if (State.WelcomeLetter > 0)
            {
                State.ReadMessages.Remove(serialId);
                State.UnreadMessages.Remove(serialId);
            }
            if (State.WelcomeLetter != serialId)
            {
                State.WelcomeLetter = serialId;
                if (serialId != 0)
                {
                    State.UnreadMessages.Add(serialId);
                    UpdateMinMax();
                }
            }
        }

        [SharedCommand]
        internal void ReplaceMessages(long[] serialIds)
        {
            serialIds = serialIds.Where(_ => _ >= State.EarliestStoredMessage).ToArray();

            State.ReadMessages = serialIds.Intersect(State.ReadMessages).ToList();
            State.UnreadMessages = serialIds.Intersect(State.UnreadMessages).ToList();

            if (serialIds.Length == 0) return;

            var newMessages = serialIds.Except(State.ReadMessages.Union(State.UnreadMessages)).ToArray();
            State.UnreadMessages = serialIds.Intersect(State.UnreadMessages).Union(newMessages).ToList();

            UpdateMinMax();
        }

        [SharedCommand]
        internal bool ClaimCustomReward(long serialId, string id, int amount)
        {
            if (!State.UnreadMessages.Contains(serialId))
                return false;
            return ClaimCustomRewardImpl(id, amount);
        }

        [SharedCommand]
        internal bool ClaimChestReward(long serialId, string chestRewardId)
        {
            if (!State.UnreadMessages.Contains(serialId))
                return false;
            return ClaimChestRewardImpl(chestRewardId);
        }

        private void UpdateMinMax()
        {
            long max = 0;
            long min = long.MaxValue;
            foreach (var id in State.UnreadMessages.Union(State.ReadMessages))
            {
                if (id < min) min = id;
                if (id > max) max = id;
            }

            if(max > State.LastSeenMessage)
                State.LastSeenMessage = max;

            State.EarliestStoredMessage = min;
        }

        protected virtual bool ClaimCustomRewardImpl(string id, int amount)
        {
            return false;
            // override
        }

        protected virtual bool ClaimChestRewardImpl(string chestRewardId)
        {
            return false;
            // override
        }

        public long[] UnreadMessages
        {
            get { return State.UnreadMessages.ToArray(); }
        }

        public long[] ReadMessages
        {
            get { return State.ReadMessages.ToArray(); }
        }

        public long[] AllMessages
        {
            get { return UnreadMessages.Union(ReadMessages).ToArray(); }
        }

        public long LastSeenMessage
        {
            get { return State.LastSeenMessage; }
        }

        public long StateBirthday
        {
            get { return State.StateBirthday; }
        }
    }
}
