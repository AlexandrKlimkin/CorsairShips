using System;
using System.Collections;
using System.Collections.Generic;
using MessagePack;
using PestelLib.Chests;
using PestelLib.Localization;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicBase;
using PestelLib.SharedLogicClient;
using S;
using PestelLib.ServerShared;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;

public class ChestManager : MonoBehaviour
{
    [Dependency] private IChestsConcreteGameInterface _chestsConcreteGameInterface;
    [Dependency] private ILocalization _localizationData;
    [Dependency] private CommandProcessor _commandProcessor;
    
    [SerializeField] private ChestScreenItem _chestScreenItemPrefab;
    [SerializeField] private RectTransform _chestScreenItemsContainer;

    [Header("Gui elements")]
    [SerializeField] private Text _caption;
    [SerializeField] private GameObject _continueButton;
    [SerializeField] private Text _tapToOpen;
    [SerializeField] private Text _tapToContinue;
    [SerializeField] private Image _screenBackground;
    [SerializeField] private Color[] _rarityColors;

    public Action OnClose = () => { };
    
	public GameObject[] chest;
	private string chestNamе = "ChestMesh"; // не менять!
	public Animator anim;
	public RuntimeAnimatorController controller;

    public int ChestNumber;
    public int ChestRarity = 0;

    
    
    public string Caption {
        set { _caption.text = value; }
    }

    void Awake()
    {
        _chestScreenItemsContainer.gameObject.SetActive(false);
        _chestScreenItemPrefab.gameObject.SetActive(false);
    }

	void Start()
	{
        ContainerHolder.Container.BuildUp(this);

		GameObject ch =  Instantiate(chest[ChestNumber], gameObject.transform );
		ch.name = chestNamе;
		anim.runtimeAnimatorController = controller as RuntimeAnimatorController;

	    _tapToOpen.text = _localizationData.Get("tap_to_open_chest");
        _tapToContinue.text = _localizationData.Get("tap_to_continue");
   
	    if (_screenBackground != null && _rarityColors != null)
	    {
	        if (ChestNumber >= 0 && ChestNumber < _rarityColors.Length)
	            _screenBackground.color = _rarityColors[ChestNumber];
	    }
    }

    public void OverrideRarityColors(Color[] colors)
    {
        _rarityColors = colors;
    }

    public void OnClickOpen()
    {
        var cmd = new ChestModule_GiveChestByRarity {chestRarity = ChestRarity};
        var rewards = CommandProcessor.Process<List<ChestsRewardDef>, ChestModule_GiveChestByRarity>(cmd);

        var aspectRatio = (float)Screen.width/(float)Screen.height;
        var wideScreen = 16f/9f;
        var normalScreen = 4f/3f;

        var coeff = Mathf.InverseLerp(wideScreen, normalScreen, aspectRatio);

        var step = Mathf.Lerp(400, 300, coeff);

        var length = step*(rewards.Count - 1);
        var startPos = -length/2;

        for (var i = 0; i < rewards.Count; i++)
        {
            var chestScreenItem = Instantiate(_chestScreenItemPrefab);
            chestScreenItem.transform.SetParent(_chestScreenItemsContainer, false);
            chestScreenItem.ChestsRewardVisualData = _chestsConcreteGameInterface.GetRewardVisualData(rewards[i]);

            var cardMover = chestScreenItem.GetComponent<CurveCardMover>();
            cardMover._Offset = startPos + i*step;
        }
        StartCoroutine(Open());
    }
    

    private IEnumerator Open()
    {
        yield return new WaitForSeconds(0.25f);
        _chestScreenItemsContainer.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.75f);
        _continueButton.SetActive(true);
    }

    public void OnClickContinue()
    {
        Destroy(gameObject);
        OnClose();
    }
}
