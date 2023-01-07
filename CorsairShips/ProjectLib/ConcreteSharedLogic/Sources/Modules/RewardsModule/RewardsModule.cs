using System.Collections.Generic;
using Game.SeaGameplay.GameModes;
using PestelLib.SharedLogicBase;
using UnityDI;

namespace PestelLib.SharedLogic.Modules {
    public class RewardsModule : SharedLogicModule<RewardsModuleState> {

        [Dependency]
        private readonly ItemsModule _ItemsModule;

        public ScheduledAction<List<RewardData>> OnRewardsClaimed;

        public RewardsModule() {
            OnRewardsClaimed = new ScheduledAction<List<RewardData>>(ScheduledActionCaller);
        }

        // public MatchResultData CalculateMatchResultData(MatchResult matchResult, int points) {
        //     var matchResultData = new MatchResultData {
        //         MatchResult = matchResult,
        //         Rewards = CalculateRewardsData(matchResult, points),
        //     };
        //     return matchResultData;
        // }

        public List<RewardData> CalculateRewardsData(Match_Result matchResult, int points) {

            var rewards = new List<RewardData>();

            var softCount = (int)(points * 0.1f);
            if (softCount > 0) {
                var softReward = new RewardData() {
                    ItemId = ItemsConstants.CurrencySoft,
                    Count = softCount,
                };
                rewards.Add(softReward);
            }

            // var hardCount = (int)(points * 0.01f);
            // if (hardCount > 0) {
            //     var hardReward = new RewardData() {
            //         ItemId = ItemsConstants.CurrencyHard,
            //         Count = hardCount,
            //     };
            //     rewards.Add(hardReward);
            // }
            return rewards;
        }

        [SharedCommand]
        internal void ClaimRewards(Match_Result matchResult, int points) {
            var rewards = CalculateRewardsData(matchResult, points);
            foreach (var reward in rewards) {
                _ItemsModule.AddItem(reward.ItemId, reward.Count);
            }
            OnRewardsClaimed.Schedule(rewards);
        }
    }
}