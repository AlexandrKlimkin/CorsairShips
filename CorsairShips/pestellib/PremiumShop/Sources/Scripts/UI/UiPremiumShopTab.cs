using System;
using MessagePack.Decoders;
using PestelLib.SharedLogic.Modules;
using UnityEngine;
using UnityEngine.UI;
using PestelLib.Localization;

public class UiPremiumShopTab : MonoBehaviour
{
    [SerializeField] protected Toggle _toggle;
    [SerializeField] protected Text _text;
    [SerializeField] protected Image _icon;
    [SerializeField] protected Image _lineImage;
    [SerializeField] protected LocalizeText _localizeText;

    [Header("Colors")]
    [SerializeField] protected Color _selectColor = Color.white;
    [SerializeField] protected Color _unselectColor = Color.gray;
    [SerializeField] protected Color _premiumColor = Color.yellow;

    public event Action OnSelected = () => { };
    public event Action OnUnselect = () => { };
    public static event Action<string> OnPremiumShopTabSelected;

    private string _tabType;
    public string TabType { get { return _tabType; } }
    protected PremiumShopTabVisualData _data;

    public void SetData(PremiumShopTabVisualData data)
    {
        _text.text = data.Name;
        _icon.sprite = data.Icon;
        _tabType = data.TabType;
        SetLocalizeKey(data.Name);
        _data = data;
    }

    public void SetToggleGroup(ToggleGroup toggleGroup)
    {
        _toggle.isOn = false;
        _toggle.group = toggleGroup;
    }

    // Use this for initialization
	void Awake ()
	{
	    _toggle.onValueChanged.AddListener(OnToggle);
        OnSelected += () => SetColorLine(_data.Name == "PremiumSub"? _premiumColor:_selectColor);
        OnUnselect += () => SetColorLine(_unselectColor);
    }

    protected void OnToggle(bool val)
    {
        if (val)
        {
            OnSelected();
        }
        else
            OnUnselect();
    }

    public void SelectToggle(bool isSelect = true, bool fromInitalization = default(bool))
    {
        _toggle.isOn = isSelect && !fromInitalization;
        if (isSelect && !fromInitalization)
        {
            OnPremiumShopTabSelected(TabType);
        }
    }

    public  void SetColorLine(Color color)
    {
        if (_lineImage != null)
            _lineImage.color = color;
    }

    public void SetLocalizeKey(string text)
    {
        if (_localizeText != null)
            _localizeText.Key = text;
    }
}
