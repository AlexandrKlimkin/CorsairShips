using System;
using System.Collections;
using System.Collections.Generic;
using PestelLib.Localization;
using PestelLib.SharedLogic.Modules;
using TMPro;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;
using UTPLib.Services.ResourceLoader;

namespace UI.Widgets.Ship {
    public class ShipWidget : MonoBehaviour {
        [Dependency]
        private readonly IResourceLoaderService _ResourceLoader;
        [Dependency]
        private readonly ILocalization _Localization;
        
        [SerializeField]
        private Image _PreviewImage;
        [SerializeField]
        private TextMeshProUGUI _NameText;
        [SerializeField]
        private Button _Button;

        public event Action<ShipWidget> OnButtonClick;
        public ShipDef ShipDef { get; private set; }
        
        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
        }

        public void Setup(ShipDef shipDef) {
            ShipDef = shipDef;
            if(shipDef == null)
                return;
            var preview = _ResourceLoader.LoadResource<Sprite>(ResourcePath.Ships.GetPreviewPath(shipDef.Id));
            _PreviewImage.sprite = preview;
            _NameText.text = _Localization.Get(shipDef.NameLocKey);
            _Button.onClick.RemoveAllListeners();
            _Button.onClick.AddListener(OnClick);
        }

        private void OnClick() {
            OnButtonClick?.Invoke(this);
        }
    }
}
