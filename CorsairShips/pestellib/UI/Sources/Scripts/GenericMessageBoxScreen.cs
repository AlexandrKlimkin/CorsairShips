using System;
using System.Collections;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using PestelLib.Utils;
using UnityEngine.EventSystems;

namespace PestelLib.UI
{
    public class GenericMessageBoxScreen : MonoBehaviour, IPointerDownHandler
    {
        public static string DefaultPrefabOverride = String.Empty;
        public static string DefaultPrefabPlayerIdOverride = String.Empty;

        public Action OnClose = () => { };
        [Dependency] protected Gui Gui;
        [Dependency] protected IPlayerIdProvider _requestQueue;

        [SerializeField] private string _enableSound;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayout;
        [SerializeField] private Text _playerId;
        [SerializeField] private GameObject _playerIdContainer;

        IMessageBoxView _viewModel;

        public IMessageBoxView viewModel => _viewModel;

        public event Action<GenericMessageBoxScreen> OnHide = (t) => { };

        private Animation _animation;

        public bool IsAutoHide
        {
            get
            {
                return _isAutoHide && !IsHIdeBlockedByAnim;
            }
            protected set { _isAutoHide = value; }
        }

        public bool IsHIdeBlockedByAnim
        {
            get
            {
                bool isAnimPlaying = _animation != null && _animation.isPlaying;
                return isAnimPlaying && _isHideBlockedByAnim;
            }
        }

        private bool _isAutoHide = false;
        private bool _isHideBlockedByAnim = false;

        private Action _onPressA = () => { };
        private Action _onPressB = () => { };
        private Action _onTapAnywhere = null;
        
        protected virtual void Awake()
        {
            IsAutoHide = true;

            CreateContext();
            //GetComponent<UIRootContext>().SetContext(_viewModel);
            _animation = GetComponent<Animation>();

            //Anim event on first frame doesn't work
            if (_animation != null && _animation.playAutomatically)
            {
                var clip = _animation.clip;
                foreach (AnimationEvent animationEvent in clip.events)
                {
                    if (animationEvent.functionName == "OnAnimEventBlockAutoHide")
                        OnAnimEventBlockAutoHide();
                }
            }
        }

        protected virtual void OnEnable()
        {
            StartCoroutine("PlaySound");
        }

        IEnumerator PlaySound()
        {
            yield return new WaitForEndOfFrame();

            if (!string.IsNullOrEmpty(_enableSound) && gameObject.activeSelf)
            {
                //MasterAudio.PlaySound(_enableSound);
            }
        }

        protected virtual void Start()
        {

        }

        protected virtual void OnDestroy()
        {

        }

        protected void OnDisable()
        {
            OnHide(this);
        }

        protected virtual void CreateContext()
        {
            _viewModel = GetComponents<MonoBehaviour>()
                .FirstOrDefault(v => v is IMessageBoxView) as IMessageBoxView;
        }

        public void Setup(
            string caption,
            string description,
            string icon,
            string buttonALabel,
            string buttonBLabel,
            Action buttonAAction,
            Action buttonBAction,
            Action tapAnywhereAction,
            bool showIdentity,
            bool cantClose = false)
        {
            ContainerHolder.Container.BuildUp(this);

            _viewModel.ShowDescription();

            _viewModel.Caption = caption;//Localization.Get(caption);
            _viewModel.Description = description;//Localization.Get(description);

            if(string.IsNullOrEmpty(description))
                _viewModel.ShowDescription(false);

            _viewModel.Icon = icon;
            _viewModel.ButtonA = buttonALabel;//Localization.Get(buttonALabel);
            _viewModel.ButtonB = buttonBLabel;//Localization.Get(buttonBLabel);
            _viewModel.CantClose = cantClose;

            _onPressA = buttonAAction;
            _onPressB = buttonBAction;
            _onTapAnywhere = tapAnywhereAction;
            string id = _requestQueue.DeviceId;
            if (showIdentity && _requestQueue != null)
            {
                var shortId = _requestQueue.ShortId;
                var playerId = _requestQueue.PlayerId;
                if (shortId > 0)
                    id = shortId.ToString();
                else if (playerId != Guid.Empty)
                    id = _requestQueue.PlayerId.ToString();
            }

            if (_playerIdContainer != null)
            {
                if (_playerIdContainer.activeSelf != showIdentity)
                    _playerIdContainer.SetActive(showIdentity);
                _playerId.text = "ID: " + id;
            }

            _viewModel.UpdateView();
            
            if (_horizontalLayout != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_horizontalLayout.GetComponent<RectTransform>());
        }

        public void SetComplexCaption(string caption, params object[] parameters)
        {
            _viewModel.Caption = string.Format(caption/*Localization.Get(caption)*/, parameters);
        }

        public void SetComplexDescription(string caption, params object[] parameters)
        {
            _viewModel.Description = string.Format(caption/*Localization.Get(caption)*/, parameters);
        }

        //If you want to rename - look at Awake.
        public void OnAnimEventBlockAutoHide()
        {
            _isHideBlockedByAnim = true;
        }

        public void OnAnimEventUnblockAutoHide()
        {
            _isHideBlockedByAnim = false;
        }

        public void SetAutoHide(bool isAutoHide, float delayToInvert = 0f)
        {
            IsAutoHide = isAutoHide;

            if (delayToInvert > 0)
                StartCoroutine(InvertAutoHideDelay(delayToInvert));
        }

        private IEnumerator InvertAutoHideDelay(float delay)
        {
            float timer = delay;
            while (timer > 0)
            {
                timer -= Time.unscaledDeltaTime;
                yield return null;
            }

            IsAutoHide = !IsAutoHide;
        }

        public static GenericMessageBoxScreen Show(string prefab = GenericMessageBoxDef.DefaultPrefabName)
        {
            var messagebox = ContainerHolder.Container.Resolve<Gui>()
                .Show<GenericMessageBoxScreen>(GetPrefabName(prefab), GuiScreenType.Dialog);
            return messagebox;
        }

        public static GenericMessageBoxScreen Show(GenericMessageBoxDef def)
        {
#pragma warning disable 612
            var win = Show(def.Caption, def.Description, def.Icon, def.ButtonALabel, def.ButtonBLabel, def.ButtonAAction, def.ButtonBAction, def.TapAnywhereAction, def.Prefab, def.ShowIdentity, def.CantClose);
            win.IsAutoHide = def.AutoHide;
            return win;
#pragma warning restore 612
        }

        private static string GetPrefabName(string prefab)
        {
            if (prefab == GenericMessageBoxDef.DefaultPrefabName)
            {
                if (string.IsNullOrEmpty(DefaultPrefabOverride))
                {
                    return prefab;
                }
                else
                {
                    return DefaultPrefabOverride;
                }
            }
            else
            {
                return prefab;
            }
        }

        private static GenericMessageBoxScreen Show(string caption,
            string description,
            string icon,
            string buttonALabel,
            string buttonBLabel = null,
            Action buttonAAction = null,
            Action buttonBAction = null,
            Action tapAnywhereAction = null,
            string prefab = GenericMessageBoxDef.DefaultPrefabName,
            bool showIdentity = false,
            bool cantClose = false)
        {
            if (buttonAAction == null) buttonAAction = () => {};
            if (buttonBAction == null) buttonBAction = () => {};
            
            var messagebox = ContainerHolder.Container.Resolve<Gui>().Show<GenericMessageBoxScreen>(GetPrefabName(prefab), GuiScreenType.Dialog);
            if (messagebox == null)
            {
                Debug.LogError("Screen is null!!!");
            }
            messagebox.Setup(caption, description, icon, buttonALabel, buttonBLabel, buttonAAction, buttonBAction, tapAnywhereAction, showIdentity, cantClose);
            return messagebox;
        }

        public static GenericMessageBoxScreen ShowError(string caption,
            string description,
            string icon,
            string buttonALabel,
            string buttonBLabel = null,
            Action buttonAAction = null,
            Action buttonBAction = null,
            Action tapAnywhereAction = null,
            string prefab = GenericMessageBoxDef.DefaultPrefabName)
        {
            if (buttonAAction == null) buttonAAction = () => {};
            if (buttonBAction == null) buttonBAction = () => {};

            var messagebox = Show(caption, description, icon, buttonALabel, buttonBLabel, buttonAAction, buttonBAction,
                tapAnywhereAction, prefab);

            messagebox.IsAutoHide = false;

            return messagebox;
        }

        public void OnPressA()
        {
            if (IsAutoHide)
            {
                Gui.Hide(gameObject);
                OnClose();
            }

            if (_onPressA != null)
                _onPressA();
        }

        public void OnPressB()
        {
            if (IsAutoHide)
            {
                Gui.Hide(gameObject);
                OnClose();
            }

            if (_onPressB != null)
                _onPressB();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_onTapAnywhere == null) return;
            
            if (TrySkipTextAnimation()) return;

            if (IsAutoHide)
            {
                Gui.Hide(gameObject);
                OnClose();
            }

            _onTapAnywhere();
        }
        
        public void Close()
        {
            if (IsAutoHide)
            {
                Gui.Close(gameObject);
                OnClose();
            }
        }

        private bool TrySkipTextAnimation()
        {
            /*if (_animatedText != null && _animatedText.IsAnimated)
            {
                _animatedText.Skip();
                return true;
            }*/

            return false;
        }
    }

    public class GenericMessageBoxDef
    {
        public const string DefaultPrefabName = "GenericMessageBoxScreen";
        public string Caption = "";
        public string Description = "";
        public string Icon = "";
        public string ButtonALabel = "";
        public string ButtonBLabel = "";
        public Action ButtonAAction = () => { };
        public Action ButtonBAction = () => { };
        public Action TapAnywhereAction = null;
        public string Prefab = DefaultPrefabName;
        public bool AutoHide = true;
        public bool ShowIdentity = false; // show player id and maybe version
        public bool CantClose = false;  // fatal error dialog, future actions not allowed, only close app. if dialog is closed somehow Application.Quit() will be called
    }
}