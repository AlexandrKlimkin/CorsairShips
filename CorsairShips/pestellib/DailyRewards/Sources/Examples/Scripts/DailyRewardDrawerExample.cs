using System.Globalization;
using PestelLib.Chests;
using PestelLib.SharedLogic.Modules;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardDrawerExample : ChestsRewardDrawerExample, IDailyRewardItem
{
    [Header("Day")]
    [SerializeField]
    private Text _txtDay;
    [SerializeField] private Image _imgBackground;
    [Header("Colors")]
    [SerializeField]
    private Color _availableColor;
    [SerializeField] private Color _claimedColor;

    public void SetData(DailyRewardsVisualData data)
    {
        ContainerHolder.Container.BuildUp(this);
        _txtDay.text = _localizationData.Get("Day") + " " + data.Day;
        //_icon.sprite = data.Icon;
        //var rewardName = "<color=" + ColorUnitls.ColorToHex(data.Color) + ">" + data.Description + "</color>";
        //_name.text = rewardName;
        _name.color = data.Color;
        _amount.color = data.Color;
        _imgBackground.color = data.Color;

        if (data.IsAvailable)
        {
            _imgBackground.color = _availableColor;
        }

        if (data.IsClaimed)
        {
            _imgBackground.color = _claimedColor;
        }
    }

    public override void Draw(ChestsRewardDef chestsRewardDef)
    {
        ContainerHolder.Container.BuildUp(this);

        switch (chestsRewardDef.ItemType)
        {
            case "soft_currency":
                _icon.sprite = _softIcon;
                _name.text = _localizationData.Get("Gold");
                _amount.text = chestsRewardDef.Amount.ToString(CultureInfo.InvariantCulture);
                break;

            case "hard_currency":
                _icon.sprite = _hardIcon;
                _name.text = _localizationData.Get("Crystal Packs");
                _amount.text = chestsRewardDef.Amount.ToString(CultureInfo.InvariantCulture);
                break;

            case "ship":
                _icon.sprite = _spritesDatabase.GetSprite(chestsRewardDef.ItemId);
                _name.text = _localizationData.Get("Ship" + chestsRewardDef.ItemId);
                //_amount.text = string.Empty;
                break;
            case "item":
                _icon.sprite = _spritesDatabase.GetSprite(chestsRewardDef.ItemId);
                _name.text = _localizationData.Get(chestsRewardDef.ItemId);
                //_amount.text = "Reward lvl: " + (chestsRewardDef.Tier + 1);
                break;
        }
    }
}
