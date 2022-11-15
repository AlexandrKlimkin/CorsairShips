using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PestelLib.SharedLogicBase;
using PestelLib.Localization;
using PestelLib.SharedLogic.Modules;
using UnityEngine;
using UnityDI;
using S;
using PestelLib.SharedLogicClient;
using log4net;

public class DailyRewardsExampleGame : MonoBehaviour, IDailyRewardsConcreteGameInterface
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(DailyRewardsExampleGame));

    [Dependency] private DailyRewardModule _dailyRewards;
    [Dependency] private ILocalization _localizationData;

    private static Color MakeTransparent(Color c, float a)
    {
        c.a = a;
        return c;
    }

    private static Color softMoneyColor = MakeTransparent(Color.yellow, 0.1f);
    private static Color hardMoneyColor = MakeTransparent(Color.blue, 0.1f);
    private static Color itemColor = MakeTransparent(Color.white, 0.1f);

    public DailyRewardsVisualData GetRewardVisualData(DailyRewardsDef dailyRewardDef)
    {
        if(_localizationData == null)
            ContainerHolder.Container.BuildUp(this);
        var currentDay = Refresh();
        var day = int.Parse(dailyRewardDef.Id);
        var lastDay = _dailyRewards.LastClaimedDay;

        switch (day % 3)
        {
            case 1:
                var softAmount = (day * 10).ToString();
                return new DailyRewardsVisualData
                {
                    Day = day,
                    IsClaimed = day <= lastDay,
                    IsAvailable = day == currentDay,
                    Description = softAmount + "x" + _localizationData.Get("Gold"),
                    Color = softMoneyColor,
                };
            case 2:
                var hardAmount = (day * 2).ToString();
                return new DailyRewardsVisualData
                {
                    Day = day,
                    IsClaimed = day <= lastDay,
                    IsAvailable = day == currentDay,
                    Description = hardAmount + "x" + _localizationData.Get("Crystal Packs"),
                    Color = hardMoneyColor,
                };
            default:
                return new DailyRewardsVisualData
                {
                    Day = day,
                    IsClaimed = day <= lastDay,
                    IsAvailable = day == currentDay,
                    Description = _localizationData.Get("Item" + dailyRewardDef.RewardId),
                    Color = itemColor,
                };
        }
    }

    public void ShowAds(Action<bool> callback)
    {
        Log.Debug("DailyRewardsExampleGame.ShowAds");
        callback(true);
    }

    public bool Claim(bool withBonus)
    {
        Log.Debug("DailyRewardsExampleGame.Claim");

        var cmd = new DailyRewardModule_ClaimRewards();
        var r = CommandProcessor.Process<bool, DailyRewardModule_ClaimRewards>(cmd);
        Log.Debug("CommandProcessor.Claim result " + r);
        return r;
    }

    public void ResetRewards()
    {
        Log.Debug("DailyRewardsExampleGame.ResetRewards");
        var cmd = new DailyRewardModule_ResetRewards();
        CommandProcessor.Process<object, DailyRewardModule_ResetRewards>(cmd);
    }

    private int Refresh()
    {
        var cmd = new DailyRewardModule_Refresh();
        return CommandProcessor.Process<int, DailyRewardModule_Refresh>(cmd);
    }
}
