using System;

namespace PestelLib.SharedLogic.Modules
{
    public interface IDailyRewardsConcreteGameInterface
    {
        DailyRewardsVisualData GetRewardVisualData(DailyRewardsDef dailyRewardDef);
        void ShowAds(Action<bool> callback);
        bool Claim(bool withBonus);
        void ResetRewards();
    }
}