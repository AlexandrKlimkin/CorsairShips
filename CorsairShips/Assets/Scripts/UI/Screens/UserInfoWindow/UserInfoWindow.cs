using System;
using PestelLib.SharedLogic.Modules;
using PestelLib.SharedLogicClient;
using PestelLib.UI;
using UnityEngine;
using TMPro;
using UnityDI;
using UnityEngine.UI;

namespace UI.Screens.UserinfoWindow {
    public class UserInfoWindow : MonoBehaviour {
        [Dependency]
        private readonly UserProfileModule _UserProfileModule;
        [Dependency]
        private readonly Gui _Gui;
        
        [SerializeField]
        private TMP_InputField  _NicknameInputField;
        [SerializeField]
        private Button _CloseButton;
        [SerializeField]
        private Button _BG_Button;

        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
        }

        private void Start() {
            _CloseButton.onClick.AddListener(Close);
            _BG_Button.onClick.AddListener(Close);
            _NicknameInputField.onEndEdit.AddListener(OnEndEditNickname);
        }

        private void OnDestroy() {
            _NicknameInputField.onEndEdit.RemoveAllListeners();
        }

        private void OnEnable() {
            _NicknameInputField.text = _UserProfileModule.Nickname;
        }

        private void Close() {
            _Gui.Hide(gameObject);
        }

        private void OnEndEditNickname(string value) {
            if(string.IsNullOrEmpty(value))
                return;
            SharedLogicCommand.UserProfileModule.ChangeNickname(value);
        }
    }
}
