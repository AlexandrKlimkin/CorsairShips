using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityDI;
using PestelLib.Localization;
using PestelLib.SharedLogic.Modules;
using UnityEngine;

public class ChestExampleGame : MonoBehaviour, IChestsConcreteGameInterface
{
    [Dependency] private ILocalization _localizationData;
    
    [SerializeField] private Sprite _testItemSprite;
    [SerializeField] private Sprite _softIcon;
    [SerializeField] private Sprite _hardIcon;

    [SerializeField] private List<Sprite> _ships;
    [SerializeField] private List<Sprite> _items;

    private void Awake()
    {
        ContainerHolder.Container.BuildUp(this);
    }
    
    public ChestsRewardVisualData GetRewardVisualData(ChestsRewardDef chestsRewardDef)
    {
        Color bgColor = new Color(1, 1, 1, 0.3f);

        switch (chestsRewardDef.ItemType)
        {
            case "soft_currency":
                return new ChestsRewardVisualData
                {
                    Icon = _softIcon,
                    Name = _localizationData.Get("Gold"),
                    Description = chestsRewardDef.Amount.ToString(CultureInfo.InvariantCulture),
                    BackgroundColor = bgColor,
                    //IconScale = new Vector3(0.5f, 0.5f, 0.5f)
                };
            case "hard_currency":
                return new ChestsRewardVisualData
                {
                    Icon = _hardIcon,
                    Name = _localizationData.Get("Crystal Packs"),
                    Description = chestsRewardDef.Amount.ToString(CultureInfo.InvariantCulture),
                    BackgroundColor = bgColor,
                    //IconScale = new Vector3(0.5f, 0.5f, 0.5f)
                };
            case "ship":
                //var ship = UOW_ShipConstructor.Instance.GetShipReference(rewardDef.ItemId);
                //bgColor = new Color(ship.ShipRef.Rarity.color.r, ship.ShipRef.Rarity.color.g, ship.ShipRef.Rarity.color.b, 0.3f);
                bgColor = new Color(0.5f, 1, 0.25f, 0.3f);
                return new ChestsRewardVisualData
                {
                    Icon = _ships.FirstOrDefault(x => x.name == chestsRewardDef.ItemId),
                    Name = _localizationData.Get("Ship" + chestsRewardDef.ItemId),
                    BackgroundColor = bgColor
                };
            case "item":
                //var item = ItemManager.database.items.FirstOrDefault(x => x.name == rewardDef.ItemId);
                //var itemDef = _pseudoDefinitions.GetItemDef(rewardDef.ItemId);
                
                bgColor = new Color(1, 0.5f, 0.25f, 0.3f);
                return new ChestsRewardVisualData
                {
                    Icon = _items.FirstOrDefault(x => x.name == chestsRewardDef.ItemId),
                    Name = _localizationData.Get(chestsRewardDef.ItemId),
                    RewardLevelString = "Reward lvl: " + (chestsRewardDef.Tier + 1),
                    BackgroundColor = bgColor
                };
        }

        return new ChestsRewardVisualData
        {
            Icon = _testItemSprite,
            Amount = chestsRewardDef.Amount,
            Name = chestsRewardDef.Id,
            Description = chestsRewardDef.Description
        };
    }
}
