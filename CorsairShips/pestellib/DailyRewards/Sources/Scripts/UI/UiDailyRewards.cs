using System.Collections.Generic;
using System.Linq;
using PestelLib.Chests;
using UnityEngine;
using UnityDI;
using PestelLib.SharedLogic.Modules;
using PestelLib.UI;
using UnityEngine.UI;

public class UiDailyRewards : MonoBehaviour
{
    [SerializeField] protected Transform _dailyRewardContainer;
    [SerializeField] protected string _style = "daily_reward";

    [Dependency] protected List<DailyRewardsDef> _dailyRewardsDefs;
    [Dependency] protected List<ChestsRewardDef> _chestRewardsDefs;
    [Dependency] protected IDailyRewardsConcreteGameInterface _gameApi;
    [Dependency] protected Gui _gui;
    [Dependency] protected NotificationGui _notificationGui;
    [Dependency] protected ChestsRewardVisualizer _ChestsRewardVisualizer;

    protected virtual void Start()
    {
        ContainerHolder.Container.BuildUp(this);
        LoadRewards();
    }

    protected void ClearItemsContainer()
    {
        foreach (Transform child in _dailyRewardContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public virtual void LoadRewards()
    {
        ClearItemsContainer();
        var count = _dailyRewardsDefs.Count;
        for (var i = 0; i < count; ++i)
        {
            var rewardDef = _dailyRewardsDefs[i];

            var chestReward = _chestRewardsDefs.FirstOrDefault(x => x.Id == rewardDef.RewardId);
            var item = _ChestsRewardVisualizer.GetRewardView(chestReward, _style);
            var data = _gameApi.GetRewardVisualData(rewardDef);
            item.transform.SetParent(_dailyRewardContainer, false);
            item.GetComponent<IDailyRewardItem>().SetData(data);
        }
    }

   


    public void OnDoubleRewardClick()
    {
        _gameApi.ShowAds(DoubleRewardResult);
    }

    public virtual void OnClaimClick()
    {
        if (_gameApi.Claim(false))
            _notificationGui.Close(this);
        // LoadRewards();
    }

    public void OnResetClick()
    {
        _gameApi.ResetRewards();
        LoadRewards();
    }

    public virtual void OnBack()
    {
        //_gui.GoBack();

        _notificationGui.Close(this);
    }

    private void DoubleRewardResult(bool success)
    {
        if (!success)
            return;
        if(_gameApi.Claim(true))
            _notificationGui.Close(this);
            //LoadRewards();
    }
}