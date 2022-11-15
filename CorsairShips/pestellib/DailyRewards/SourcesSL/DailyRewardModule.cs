using System;
using System.Collections.Generic;
using System.Linq;
using PestelLib.Serialization;
using PestelLib.SharedLogicBase;
using UnityDI;
using ServerShared;

namespace PestelLib.SharedLogic.Modules
{
    [System.Reflection.Obfuscation(ApplyToMembers=false)]
    public class DailyRewardModule : SharedLogicModule<DailyRewardState>
    {
        [GooglePageRef("DailyRewards")] [Dependency] protected Dictionary<string, DailyRewardsDef> _dailyRewardsDict;

        public ScheduledAction OnNoRewards;
        public ScheduledAction<int> OnRewardCantBeClaimed;

        public DailyRewardModule()
        {
            OnNoRewards = new ScheduledAction(ScheduledActionCaller);
            OnRewardCantBeClaimed = new ScheduledAction<int>(ScheduledActionCaller);
        }

        public int LastClaimedDay { get { return State.RewardStreak; } }
        public long LastClaimedTime { get { return State.LastRewardTime; } }

        public override void MakeDefaultState()
        {
            base.MakeDefaultState();
          
        }

        [SharedCommand]
        internal void ResetRewards()
        {
            Reset();
        }

        protected virtual void Reset()
        {
            State.RewardStreak = 0;
        }

        [SharedCommand]
        internal ChestsRewardDef ClaimRewards(bool withBonus)
        {
            var day = GetAvailableReward();
            if (day == 0)
            {
                OnNoRewards.Schedule();
                return null;
            }
            if (day - State.RewardStreak != 1)
            {
                OnRewardCantBeClaimed.Schedule(day);
                return null;
            }

            var rewardKey = day.ToString();
            UniversalAssert.IsTrue(_dailyRewardsDict.ContainsKey(rewardKey), "No rewards defined for day " + day);

            var rewardDef = _dailyRewardsDict[rewardKey];

            ChestsRewardDef chestsRewardDef = GiveRewards(rewardDef, withBonus ? 2 : 1);

            if (chestsRewardDef == null)
            {
                OnRewardCantBeClaimed.Schedule(day);
                return null;
            }

            State.LastRewardTime = CommandTimestamp.Ticks;
            State.RewardStreak = day;
            return chestsRewardDef;
        }
        
        protected virtual ChestsRewardDef GiveRewards(DailyRewardsDef reward, int multiplier)
        {
            Log(string.Format("Claimed reward {0}", reward.RewardId));
            return null;
        }

        [SharedCommand]
        internal int Refresh()
        {
            return GetAvailableReward();
        }

        protected virtual int GetAvailableReward()
        {
            UniversalAssert.IsNotNull(_dailyRewardsDict, "No rewards definitions");
            if (!_dailyRewardsDict.Any())
            {
                return 0;
            }
            if (State.LastRewardTime == 0)
            {
                return 1;
            }

            var diff = CommandTimestamp - new DateTime(State.LastRewardTime);
            var days = (int)(Math.Abs(diff.TotalHours) / 24);
            if (days == 0)
            {
                return 0;
            }

            if (days >= 1 && days < 2)
            {
                if (State.RewardStreak == _dailyRewardsDict.Count)
                {
                    State.RewardStreak = 0;
                    return 1;
                }

                return State.RewardStreak + 1;
            }

            if (days >= 2)
            {
                State.RewardStreak = 0;
                return 1;
            }

            return 0;
        }
    }
}
