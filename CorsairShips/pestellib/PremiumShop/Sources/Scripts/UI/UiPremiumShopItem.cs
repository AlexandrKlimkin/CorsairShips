using System;
using UnityEngine;
using UnityEngine.UI;

public class UiPremiumShopItem : MonoBehaviour
{
    [SerializeField] protected Text _name;
    [SerializeField] protected Text _description;
    [SerializeField] protected Text _price;
    [SerializeField] protected Image _icon;
    [SerializeField] protected Button _button;

    public event Action OnClick = () => { };
    public bool isIngamePrice;

    public virtual void Init(PremiumShopItemVisualData data)
    {
        isIngamePrice = false;
        if (_name != null)
            _name.text = data.Name;

        if (_description != null)
            _description.text = data.Description;

        if (_price != null)
            _price.text = data.Price;

        if (_icon != null)
            _icon.sprite = data.Icon;

        if (_button != null)
            _button.onClick.AddListener(() => OnClick());

        OnClick += () => OnClickBuy(data);
    }

    protected virtual void OnClickBuy(PremiumShopItemVisualData data)
    {
        
    }
}
