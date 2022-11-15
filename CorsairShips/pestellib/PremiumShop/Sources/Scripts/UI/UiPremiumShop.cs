using System.Collections.Generic;
using System.Linq;
using PestelLib.PremiumShop;
using PestelLib.SharedLogic.Modules;
using PestelLib.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityDI;

public class UiPremiumShop : MonoBehaviour
{
    [SerializeField] protected GameObject _tabsContainer;
    [SerializeField] protected GameObject _tabsBodyContainer;
    [SerializeField] protected UiPremiumShopTab _tabPrefab;
    [SerializeField] protected UiPremiumShopTabBody _tabBodyPrefab;
    [SerializeField] protected ToggleGroup _tabGroup;

    [Dependency] protected List<PremiumShopItemDef> _shopDefs;
    [Dependency] protected SpritesDatabase _spritesDatabase;
    [Dependency] protected Purchaser _purchaser;

    protected virtual void Start () {
        ContainerHolder.Container.BuildUp(this);
        Load();
        _purchaser.OnProductsLoaded += OnProductsLoaded;
    }

    protected virtual void OnDestroy()
    {
        _purchaser.OnProductsLoaded -= OnProductsLoaded;        
    }

    protected void OnProductsLoaded()
    {
        Load();
    }

    protected virtual void Load()
    {
        Clean();
        foreach (var g in _shopDefs.GroupBy(d => d.TabId))
        {
            var items = new List<PremiumShopItemVisualData>();
            PremiumShopItemDef itemDef = null;
            foreach (var d in g)
            {
                itemDef = d;
                items.Add(GetTabItemVisualData(d));
            }
            var tab = GetTabVisualData(itemDef);
            CreateTab(tab, items.ToArray());
        }

        SelectTab(_shopDefs[0].TabId);
    }

    protected virtual UiPremiumShopTab CreateTab(PremiumShopTabVisualData data, params PremiumShopItemVisualData[] items)
    {
        var tab = Instantiate(_tabPrefab, _tabsContainer.transform);
        var tabBody = Instantiate(_tabBodyPrefab, _tabsBodyContainer.transform);
        tab.SetToggleGroup(_tabGroup);
        tabBody.gameObject.SetActive(false);
        tabBody.AddItems(true, items);
        tabBody.OnBuy += OnBuyItem;
        tab.OnSelected += () =>
        {
            tabBody.gameObject.SetActive(true);
            TabSelected(data.Name);
        };
            
        tab.OnUnselect += () => tabBody.gameObject.SetActive(false);
        tab.SetData(data);
        return tab;
    }

    protected virtual void TabSelected(string tab)
    {
        // for override
    }

    protected virtual void OnBuyItem(PremiumShopItemVisualData item)
    {
        Debug.Log("Buy item " + item.Name);
        _purchaser.BuyProductID(item.Def.SkuId);
    }

    public virtual void SelectTab(string tab, bool fromInitialisation = default(bool))
    {
        for (int i = 0; i < _tabsContainer.transform.childCount; i++)
        {
            UiPremiumShopTab shopTab = _tabsContainer.transform.GetChild(i).GetComponent<UiPremiumShopTab>();

            if (shopTab.TabType == tab)
            {
                shopTab.SelectToggle(true, fromInitialisation);
            }
        }
    }

    protected void Clean()
    {
        foreach (Transform t in _tabsContainer.transform)
        {
            Destroy(t.gameObject);
        }

        foreach (Transform t in _tabsBodyContainer.transform)
        {
            Destroy(t.gameObject);
        }
    }

    protected virtual PremiumShopTabVisualData GetTabVisualData(PremiumShopItemDef def)
    {
        return new PremiumShopTabVisualData
        {
            Icon = _spritesDatabase.GetSprite(def.Icon),
            Name = def.TabId,
            TabType = def.TabId
        };
    }

    protected virtual PremiumShopItemVisualData GetTabItemVisualData(PremiumShopItemDef def)
    {
        return new PremiumShopItemVisualData
        {
            Name = def.Name,
            Description = def.Description,
            Price = _purchaser.GetPriceString(def.SkuId),
            Icon = _spritesDatabase.GetSprite(def.Icon),
            Def = def,
        };
    }
}
