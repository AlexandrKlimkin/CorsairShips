using System.Globalization;
using PestelLib.Chests;
using PestelLib.Localization;
using PestelLib.SharedLogic.Modules;
using PestelLib.UI;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;

public class ChestsRewardDrawerExample : ChestsRewardDrawer {
    [Dependency] protected ILocalization _localizationData;
    [Dependency] protected SpritesDatabase _spritesDatabase;
    
    [SerializeField] protected Sprite _softIcon;
    [SerializeField] protected Sprite _hardIcon;

    [SerializeField] protected Text _name;
    [SerializeField] protected Image _icon;
    [SerializeField] protected Text _amount;

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
