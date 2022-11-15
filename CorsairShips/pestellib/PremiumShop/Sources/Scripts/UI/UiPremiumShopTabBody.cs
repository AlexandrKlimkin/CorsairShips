using System;
using UnityEngine;

public class UiPremiumShopTabBody : MonoBehaviour
{
    [SerializeField] protected UiPremiumShopItem _itemPrefab;
    [SerializeField] protected GameObject _itemsContainer;

    public virtual event Action<PremiumShopItemVisualData> OnBuy = (d) => { };

    public virtual void AddItems(bool clear, params PremiumShopItemVisualData[] items)
    {
        if(clear)
            Clear();

        foreach (var item in items)
        {
            var itemObj = Instantiate(_itemPrefab, _itemsContainer.transform);
            var i = item;
            itemObj.OnClick += () => OnBuy(i);
            itemObj.Init(item);
        }
    }

    protected void Clear()
    {
        foreach (Transform t in _itemsContainer.transform)
        {
            if (t.GetComponent<UiPremiumShopItem>() != null)
                Destroy(t.gameObject);
        }
    }
}
