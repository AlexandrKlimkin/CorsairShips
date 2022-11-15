using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI.Sources;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace PestelLib.UI
{
    public class Gui : MonoBehaviour, IGui
    {
        [SerializeField] private bool _useTweenToSwitchScreen = false;
        /* _popUpBackground remained from Old projects*/
        [SerializeField] private CanvasGroupFader _popUpBackground;
        [SerializeField] private PopUpBackgroundSetup _optionalPopUpBackgroundSetup = new PopUpBackgroundSetup();
        [SerializeField] private Canvas _canvasPrefab;
        [SerializeField] private EventSystem _eventSystemPrefab;
        [SerializeField] private CanvasSetup _optionalCanvasSetup = new CanvasSetup();
        [SerializeField] private bool _forceCreateCanvas;
        [SerializeField] private bool _overrideSortingOrder = true; //false for popUpCanvas to render particles

        [SerializeField] private string[] _semiBattleScreens =
        {
            "PvpOpponentsScreen",
            "FixtureListScreen",
            "RegistrationScreen",
            "Matchmaking2",
            "MapScreen"
        };

        [SerializeField] private string[] _rootScreens =
        {
            "MainMenuScreen",
            "RegistrationScreen"
        };

        List<Type> _dontStack = new List<Type>();

        [Header("Optional camera UI prefab")] [SerializeField] private Camera _cameraPrefab;
        [Header("Particles camera UI prefab")] [SerializeField] private Camera _particlesCameraPrefab;
        private CanvasGroupFader _popUpBackgroundInstance;
        public Action<Component, Component> OnChangeScreen = (from, to) => { };
        public Action<GameObject, ScreenEvent> OnScreenEvent = (screen, evt) => { };
        public Action BackButtonPressed = () => { };

        private List<GuiScreenData> _screens = new List<GuiScreenData>();
        private List<GuiScreenData> _stack = new List<GuiScreenData>();
        private Canvas _canvas;
        private Canvas _popUpCanvas;
        private static int _serialNumberCounter;
        private Camera _uiCam;

        private GuiScreenData _focusedScreen;

        private void Awake()
        {
            if (_popUpBackground != null)
            {
                _popUpBackgroundInstance = Instantiate(_popUpBackground);
            }
            else
            {
                _popUpBackgroundInstance = PopUpBackgroundBuilder.MakePopUpBackground(_optionalPopUpBackgroundSetup);
            }

            if (_eventSystemPrefab != null)
            {
                var eventSystemInstance = Instantiate(_eventSystemPrefab);
                DontDestroyOnLoad(eventSystemInstance);
            }

            StartCoroutine(SetCanvasToScreen(_popUpBackgroundInstance.gameObject));
            _popUpBackgroundInstance.SetActiveNow(false);

            if (_cameraPrefab != null)
            {
                _uiCam = Instantiate(_cameraPrefab);
                DontDestroyOnLoad(_uiCam);
                Canvas.worldCamera = _uiCam;
                Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            }
        }

        public CanvasGroupFader PopUpBackgroundInstance
        {
            get { return _popUpBackgroundInstance; }
        }

        public Camera UiCamera {
            get { return _uiCam; }
        }

        private Canvas MakeNewCanvas()
        {
            if (_canvasPrefab != null)
            {
                return Instantiate(_canvasPrefab);
            }
            else
            {
                return CanvasBuilder.CreateCanvas(_optionalCanvasSetup);
            }
        }

        public Canvas PopUpCanvas
        {
            get
            {
                if (_popUpCanvas == null)
                {
                    _popUpCanvas = MakeNewCanvas();
                    
                    if(_overrideSortingOrder)
                        _popUpCanvas.sortingOrder += 10;
                    
                    if (_particlesCameraPrefab != null)
                    {
                        Camera particlesCamera = Instantiate(_particlesCameraPrefab);
                        DontDestroyOnLoad(particlesCamera);
                        _popUpCanvas.worldCamera = particlesCamera;
                        _popUpCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                        _popUpCanvas.sortingLayerName = "Particles"; //layer in particle system
                    }
                    DontDestroyOnLoad(_popUpCanvas.gameObject);
                }
                return _popUpCanvas;
            }
        }

        public Canvas Canvas
        {
            get
            {
                if (_canvas == null && !_forceCreateCanvas)
                {
                    var canvases = FindObjectsOfType<Canvas>();
                    for (int i = 0; i < canvases.Length; i++)
                    {
                        if (canvases[i].gameObject.layer == LayerMask.NameToLayer("UI") &&
                            canvases[i].transform.parent == null)
                        {
                            _canvas = canvases[i];

                            DontDestroyOnLoad(_canvas.gameObject);
                            if (_canvas.worldCamera != null)
                            {
                                DontDestroyOnLoad(_canvas.worldCamera.gameObject);
                            }

                            break;
                        }
                    }
                }
                if (_canvas == null)
                {
                    _canvas = MakeNewCanvas();

                    DontDestroyOnLoad(_canvas.gameObject);
                    if (_canvas.worldCamera != null)
                    {
                        DontDestroyOnLoad(_canvas.worldCamera.gameObject);
                    }
                }
                return _canvas;
            }
        }

        public T Show<T>() where T : Component
        {
            return Show<T>(GuiScreenType.Screen);
        }

        //show by window class type, generic version
        public T Show<T>(GuiScreenType type, bool preload = false) where T : Component
        {
            return Show(typeof (T).Name, typeof (T), type, preload).GetComponent<T>();
        }

        public Component Show(Type windowClass)
        {
            return Show(windowClass, GuiScreenType.Screen);
        }

        //show by window class type
        public Component Show(Type windowClass, GuiScreenType type, bool preload = false)
        {
            var result = Show(windowClass.Name, windowClass, type, preload);
            return result.GetComponent(windowClass);
        }

        public T Show<T>(string overridedPrefabName) where T : Component
        {
            return Show<T>(overridedPrefabName, GuiScreenType.Screen);
        }

        public T Show<T>(string overridedPrefabName, GuiScreenType type, bool preload = false)
            where T : Component
            //show by prefab name, return specified component
        {
            return Show(overridedPrefabName, typeof (T), type, preload).GetComponent<T>();
        }

        //method to avoid memory allocations in typeof(T).Name
        public T Get<T>(string screenTypeName) where T : MonoBehaviour
        {
            for (var i = 0; i < _screens.Count; i++)
            {
                if (_screens[i].Name == screenTypeName)
                {
                    return _screens[i].GameObject.GetComponent<T>();
                }
            }
            return null;
        }

        public T Get<T>() where T : MonoBehaviour
        {
            return Get<T>(typeof (T).Name);
        }

        public bool IsScreenActive<T>() where  T : MonoBehaviour
        {
            T screen = Get<T>();

            if (screen == null)
                return false;

            return screen.gameObject.activeSelf;
        }

        public void Close<T>() where T : MonoBehaviour
        {
            var obj = Get<T>();
            if (obj != null)
            {
                Close(obj.gameObject);
            }
        }

        public void FocusScreen<T>() where T : MonoBehaviour
        {
            var screen = Get<T>();
            if (screen == null)
            {
                screen = Show<T>();
            }

            _focusedScreen = _screens.FirstOrDefault(x => x != null && x.Screen == screen);
            SortScreens();
        }

        public void ResetFocusScreen(bool sort = true)
        {
            _focusedScreen = null;
            if (sort)
            {
                SortScreens();
            }
        }

        public void Close(GameObject screen)
        {
            var screenData = _screens.FirstOrDefault(x => x != null && x.GameObject == screen);
            if (screenData != null)
            {
                _screens.Remove(screenData);
                SendScreenEvent(screenData.GameObject, ScreenEvent.Close);
                Destroy(screenData.GameObject);
            }
            SortScreens();
        }

        public void Hide<T>(bool preload = false) where T : MonoBehaviour
        {
            var obj = Get<T>();
            if (obj != null)
            {
                Hide(obj.gameObject, preload);
            }
        }

        public void Hide(GameObject screen, bool preload = false)
        {
            SendScreenEvent(screen, preload ? ScreenEvent.HidePreload : ScreenEvent.Hide);
            if (_useTweenToSwitchScreen)
            {
                ApplyHideAnim(screen);
            }
            else
            {
                screen.SetActive(false);
                SortScreens();
            }
        }

        private IEnumerator SetCanvasToScreen(GameObject screen)
        {
            //yield return new WaitForEndOfFrame();
            if (screen != null)
            {
                screen.transform.SetParent(Canvas.transform, false);
            }
            SortScreens();
            yield return null;
        }

        private void SendScreenEvent(GameObject go, ScreenEvent evt)
        {
            var eventReceiver = go.GetComponent<IScreen>();
            if (eventReceiver != null)
            {
                eventReceiver.OnScreenEvent(evt);
            }
            OnScreenEvent(go, evt);
        }

        //show by prefab name
        private GameObject Show(string screenPrefabName, Type screenClass, GuiScreenType type, bool preload)
        {
            if (type == GuiScreenType.Screen && TopScreen != null && TopScreen.GetType() == screenClass)
            {
                return TopScreen.gameObject;
            }

            GameObject screen = null;
            GuiScreenData screenData = null;

            var oldScreen = TopScreen;

            if (type == GuiScreenType.Screen || type == GuiScreenType.InBattleScreen)
            {
                if (oldScreen != null)
                {
                    Hide(oldScreen.gameObject, preload);
                }
            }

            var hiddenScreen = _screens.FirstOrDefault(x => x != null && x.GameObject != null && x.GameObject.name == screenPrefabName && !x.GameObject.activeSelf);

            if (!type.AllowDuplicates) //don't allow to duplicate screens with type 'Screen' or 'Loading' or 'Overlay' or 'Tooltip'
            {
                hiddenScreen = _screens.FirstOrDefault(x => x != null && x.GameObject != null && x.GameObject.name == screenPrefabName);
            }

            if (hiddenScreen != null)
            {
                var oldSCreenData = _screens.FirstOrDefault(s => s.SerialNumber == hiddenScreen.SerialNumber);
                if (oldSCreenData != null && type != oldSCreenData.Type)
                    oldSCreenData.Type = type;

                hiddenScreen.SerialNumber = ++_serialNumberCounter;
                screen = hiddenScreen.GameObject;
                screen.SetActive(true);
                screenData = new GuiScreenData
                {
                    GameObject = screen,
                    Type = type,
                    Name = screenPrefabName,
                    ScreenClass = screenClass,
                    Screen = screen.GetComponent(screenClass),
                    SerialNumber = _serialNumberCounter
                };
                AddToStack(screenData);
            }
            else
            {
                Object screenObject = Resources.Load("Screens/" + screenPrefabName);
                if (screenObject == null)
                {
                    Debug.LogError("Cannot find screen " + screenPrefabName);
                    return null;
                }

                screen = Instantiate(screenObject) as GameObject;

                screen.name = screenPrefabName;
                _serialNumberCounter++;

                screenData = new GuiScreenData
                {
                    GameObject = screen,
                    Type = type,
                    Name = screenPrefabName,
                    ScreenClass = screenClass,
                    Screen = screen.GetComponent(screenClass),
                    SerialNumber = _serialNumberCounter
                };

                _screens.Add(screenData);
                AddToStack(screenData);
            }

            SendScreenEvent(screen, preload ? ScreenEvent.ShowPreload : ScreenEvent.Show);

            var screenAnimator = screen.GetComponent<Animator>();

            if (screenAnimator != null)
            {
                StartCoroutine(SetCanvasToScreen(screen));
            }
            else
            {
                screen.transform.SetParent(Canvas.transform, false);
                SortScreens();
            }

            if (type == GuiScreenType.Screen || type == GuiScreenType.InBattleScreen)
            {
                OnChangeScreen(oldScreen, screen.GetComponent(screenClass));
                
                TryApplyShowAnim(screen, SortScreens);
            }
            else
            {
                TryApplyShowAnim(screen, () => { });
            }
            return screen;
        }

        public const float TweenTimeShow = 0.6f;
        public const float TweenTimeHide = 0.2f;
        private bool _isFirstScreen = true;

        private void TryApplyShowAnim(GameObject screen, Action onComplete)
        {
            if (!_useTweenToSwitchScreen) return;

            if (_isFirstScreen)
            {
                _isFirstScreen = false;
                return;
            }

            var canvasGroup = GetScreenCanvasGroup(screen);
            canvasGroup.alpha = 0;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            //todo: add tweener?
            canvasGroup.alpha = 1;
            onComplete();
            /*
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1, TweenTimeShow).SetUpdate(true).OnComplete(() => onComplete());
            */
        }

        private void ApplyHideAnim(GameObject screen)
        {
            if (!_useTweenToSwitchScreen) return;

            var canvasGroup = GetScreenCanvasGroup(screen);
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 1;


            //TODO: add tweener?
            canvasGroup.alpha = 0;
            screen.SetActive(false);
            SortScreens();
            /*
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0, TweenTimeHide).SetUpdate(true).OnComplete(() =>
            {
                screen.SetActive(false);
                SortScreens();
            });
            */
        }

        private CanvasGroup GetScreenCanvasGroup(GameObject screen)
        {
            var canvasGroup = screen.GetComponent<CanvasGroup>();
            if (canvasGroup == null) {
                canvasGroup = screen.AddComponent<CanvasGroup>();
            }
            return canvasGroup;
        }


        public Component TopDialog
        {
            get
            {
                var lastOrDefault =
                    _screens.LastOrDefault(x => (x.Type == GuiScreenType.Dialog) && x.GameObject.activeInHierarchy);

                return (lastOrDefault != null) ? lastOrDefault.Screen : null;
            }
        }

        public Component TopScreen
        {
            get
            {
                var lastOrDefault =
                    _screens.LastOrDefault(x => 
                        (x.Type == GuiScreenType.Screen || x.Type == GuiScreenType.InBattleScreen) 
                        && x.GameObject.activeSelf);

                return (lastOrDefault != null) ? lastOrDefault.Screen : null;
            }
        }
        
        public Component TopOverlay
        {
            get
            {
                var lastOrDefault =
                    _screens.LastOrDefault(x => 
                        (x.Type == GuiScreenType.Overlay) 
                        && x.GameObject.activeSelf);

                return (lastOrDefault != null) ? lastOrDefault.Screen : null;
            }
        }
        
        public bool IsLoading
        {
            get
            {
                var lastOrDefault =
                    _screens.LastOrDefault(x => 
                        (x.Type == GuiScreenType.Loading) 
                        && x.GameObject.activeSelf);

                return lastOrDefault != null;
            }
        }

        public Component TopScreenOfType(GuiScreenType screenType)
        {
            var lastOrDefault =
                _screens.LastOrDefault(x => (x.Type == screenType) 
                    && x.GameObject.activeSelf);

            return (lastOrDefault != null) ? lastOrDefault.Screen : null;
        }

        public bool AnyVisibleDialog
        {
            //get { return _screens.Any(x => x.Type == GuiScreenType.Dialog && x.GameObject.activeInHierarchy); }
            get
            {
                int length = _screens.Count;
                for (int i = 0; i < length; i++)
                {
                    var screen = _screens[i];
                    if (screen.Type == GuiScreenType.Dialog && screen.GameObject.activeSelf)
                        return true;
                }

                return false;
            }
        }

        private void SortScreens()
        {
            try
            {
                _screens.Sort((a, b) =>
                {
                    if (a == _focusedScreen)
                        return 1;

                    if (b == _focusedScreen)
                        return -1;

                    int typeCompare = a.Type.CompareTo(b.Type);
                    if (typeCompare == 0)
                    {
                        return a.SerialNumber.CompareTo(b.SerialNumber);
                    }

                    return typeCompare;
                });

                for (var i = 0; i < _screens.Count; i++)
                {
                    if (_screens[i] == null || _screens[i].GameObject == null) continue;

                    if (_screens[i].GameObject.activeSelf)
                    {
                        if (_screens[i].GameObject.transform.GetSiblingIndex() != i)
                        {
                            _screens[i].GameObject.transform.SetSiblingIndex(i);
                        }
                    }
                }

                var allDialogs = _screens.Where(x => x != null && x.Type == GuiScreenType.Dialog);
                if (allDialogs.Any())
                {
                    var activePopUps = allDialogs.Where(x => x.GameObject != null && x.GameObject.activeSelf).ToList();
                    if (activePopUps.Count > 0)
                    {
                        for (var i = 0; i < activePopUps.Count - 1; i++)
                        {
                            activePopUps[i].GameObject.transform.SetParent(Canvas.transform, false);
                        }

                        if (TopDialog != null)
                        {
                            TopDialog.transform.SetParent(PopUpCanvas.transform, false);
                            TopDialog.transform.SetAsLastSibling();
                        }

                        var allOverlays = _screens.Where(x =>
                            x != null && x.GameObject != null && x.GameObject.activeSelf &&
                            x.Type == GuiScreenType.Tooltip).ToList();
                        if (allOverlays.Count > 0)
                        {
                            TopScreenOfType(GuiScreenType.Tooltip).transform.SetParent(PopUpCanvas.transform, false);
                            TopScreenOfType(GuiScreenType.Tooltip).transform.SetAsLastSibling();
                        }

                        _popUpBackgroundInstance.transform.SetAsLastSibling();
                        _popUpBackgroundInstance.SetActive(true);
                    }
                    else
                    {
                        _popUpBackgroundInstance.transform.SetSiblingIndex(0);
                        _popUpBackgroundInstance.SetActive(false);
                    }
                }
                else
                {
                    _popUpBackgroundInstance.transform.SetSiblingIndex(0);
                    _popUpBackgroundInstance.SetActive(false);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Unable to sort screens: " + e.Message + " " + e.StackTrace);
                return;
            }
        }

        public void Ignore(Type screen)
        {
            _dontStack.Add(screen);
        }

        private void AddToStack(GuiScreenData screen)
        {
            if (!screen.Type.Stackable || _dontStack.Contains(screen.Screen.GetType())) return;

            if (Array.IndexOf(_rootScreens, screen.Name) != -1) ClearStack();

            _stack.Add(screen);

            if (_stack.Count < 2) return;

            var last = _stack.Count-1;

            if (_stack[last].Name == _stack[last - 1].Name)
            {
                _stack.RemoveAt(last);
            }
        }

        public void GoBack()
        {
            PopStack();
        }

        private void PopStack()
        {
            if (_stack.Count <= 1) return;

            var last = 0;
            GuiScreenData screen;

            if (_stack.Count == 1)
            {
                screen = _stack[last];
            }
            else
            {
                last = _stack.Count - 1;
                screen = _stack[last - 1];
            }

            var hasBackButton = _stack[last].Screen as IBackButtonOverride;
            if (hasBackButton != null)
            {
                hasBackButton.OnBackButtonPressed();
            }
            else
            {
                _stack.RemoveAt(last);
                Show(screen.Name, screen.ScreenClass, screen.Type, false);
            }
        }

        public string GetPreviousScreen()
        {
            if (_stack.Count <= 1) return string.Empty;

            var last = 0;
            GuiScreenData screen;

            if (_stack.Count == 1)
            {
                screen = _stack[last];
            }
            else
            {
                last = _stack.Count - 1;
                screen = _stack[last - 1];
            }

            return screen.Name;
            /*
            var hasBackButton = _stack[last].Screen as IBackButtonOverride;
            if (hasBackButton != null)
            {
                hasBackButton.OnBackButtonPressed();
            }
            else
            {
                _stack.RemoveAt(last);
                Show(screen.Name, screen.ScreenClass, screen.Type, false);
            }
            */
        }

        private void ClearStack()
        {
            _stack.Clear();
        }

        public void ClearSemiBattleScreenIfExists()
        {
            foreach (var screenData in _stack.ToArray())
            {
                if (Array.IndexOf(_semiBattleScreens, screenData.Name) != -1)
                {
                    _stack.Remove(screenData);
                }
            }

            if (_stack.Count < 2) return;

            var last = _stack.Count - 1;
            if (_stack[last].Name == _stack[last - 1].Name)
            {
                _stack.RemoveAt(last);
            }
        }

        public void CloseAllWindows()
        {
            var screensCopy = _screens.ToList();
            screensCopy.ForEach(_ => Close(_.GameObject));
        }
    }
}