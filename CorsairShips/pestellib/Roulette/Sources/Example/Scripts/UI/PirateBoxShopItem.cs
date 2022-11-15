using PestelLib.Utils;
using System;
using System.Collections;
using PestelLib.ServerClientUtils;
using PestelLib.SharedLogic.Modules;
using PestelLib.UI;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;

namespace Submarines
{
    public class PirateBoxShopItem : MonoBehaviour
    {
        [SerializeField] private Image _boxItem;
        [SerializeField] private Text _boxCost;
        [SerializeField] private Image _boxCostIcon;
        [SerializeField] private Button _freeBoxButton;
        [SerializeField] private Text _freeButtonCooldownText;
        [SerializeField] private GameObject _enabledButtonState;
       
        [Dependency] private RouletteEventsModule _rouletteEventsModule;
        [Dependency] private SharedTime _sharedTime;
        [Dependency] private SpritesDatabase _spritesDatabase;

        public Action<PirateBoxChestDef> BoxBuyClicked;
        public Action<PirateBoxChestDef> BoxForFreeClicked;

        protected PirateBoxChestDef _box;

        protected virtual void Awake()
        {
            ContainerHolder.Container.BuildUp(this);
        }

        protected virtual void OnEnable()
        {
            if ((_box != null) && (gameObject.activeInHierarchy))
                SetFreeButton();
        }

        public virtual void Initialize(PirateBoxChestDef box, bool isReady = true)
        {
            _box = box;

            _boxItem.sprite = _spritesDatabase.GetSprite(_box.Name);
            _boxCost.text = box.CostKeys.ToString();

            if (gameObject.activeInHierarchy)
                SetFreeButton();
        }

        public void OnBoxBuy()
        {
            if (BoxBuyClicked != null)
                BoxBuyClicked(_box);
        }

        public void OpenBoxForFree()
        {
            if (BoxForFreeClicked != null)
                BoxForFreeClicked(_box);
        }


        protected void SetFreeButton(bool isReady = true)
        {
            _freeBoxButton.gameObject.SetActive(_box.ForFree&&isReady);
            if (_box.ForFree && isReady)
                StartCoroutine(UpdateMessageRoutine());
        }

        private IEnumerator UpdateMessageRoutine()
        {
            while (true)
            {
                UpdateButton();
                yield return new WaitForSeconds(1f);
            }
        }

        private void UpdateButton()
        {
            var remainTime = _rouletteEventsModule.GetAdsCooldown(_sharedTime.Now);
            var isEnabled = remainTime.Ticks <= 0;
            _freeBoxButton.interactable = isEnabled;
            _enabledButtonState.SetActive(isEnabled);
            _freeButtonCooldownText.enabled = !isEnabled;

            if (!isEnabled)
                _freeButtonCooldownText.text = FormatTime.Format(remainTime);
        }
    }
}
