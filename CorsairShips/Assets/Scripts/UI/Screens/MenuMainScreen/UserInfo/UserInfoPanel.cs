using System;
using System.Collections;
using System.Collections.Generic;
using PestelLib.SharedLogic.Modules;
using PestelLib.UI;
using TMPro;
using UI.Screens.UserinfoWindow;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;

namespace Menu.UserInfo {
    public class UserInfoPanel : MonoBehaviour {
        [Dependency]
        private readonly UserProfileModule _UserProfileModule;
        [Dependency]
        private readonly Gui _Gui;
        
        [SerializeField]
        private TextMeshProUGUI _NicknameText;
        [SerializeField]
        private Button _NicknameButton;

        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
        }

        private void Start() {
            _UserProfileModule.OnNicknameChanged.Subscribe(OnNicknameChanged);
            _NicknameButton.onClick.AddListener(OnNicknameButtonClick);

            _NicknameText.text = _UserProfileModule.Nickname;
        }

        private void OnDestroy() {
            _UserProfileModule.OnNicknameChanged.Unsubscribe(OnNicknameChanged);
            _NicknameButton.onClick.RemoveAllListeners();
        }

        private void OnNicknameButtonClick() {
            _Gui.Show<UserInfoWindow>(GuiScreenType.Dialog);
        }

        private void OnNicknameChanged(string nickname) {
            _NicknameText.text = nickname;
        }
    }
}
