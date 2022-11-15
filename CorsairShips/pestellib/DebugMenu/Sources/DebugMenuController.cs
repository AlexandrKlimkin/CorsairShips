using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DebugMenu
{
    public class DebugMenuController : MonoBehaviour
    {
        public static DebugMenuController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("DebugMenuController").AddComponent<DebugMenuController>();
                    DontDestroyOnLoad(_instance.gameObject); 
                }

                return _instance;
            }
        }

        private static DebugMenuController _instance;

        private class Tab
        {
            public string Name;
            public bool IsFolded;
            public Action OnGuiCallback;
        }

        private List<Tab> _tabs = new List<Tab>();
        private bool _isFolded = true;
        private Vector2 _scrollPosition;
        private int _lastTouchCount = 0;
        bool _debugMenuEnabled;

        public bool DebugMenuEnabled
        {
            get
            {
                return _debugMenuEnabled;
            }
        }

        private float _panelWidth;

        public void AddMenuTab(string name, Action callback)
        {
            if (_tabs.Find(x => x.Name == name) == null)
            {
                _tabs.Add(new Tab {Name = name, OnGuiCallback = callback});
            }
        }

        public void RemoveMenuTab(Action callback)
        {
            _tabs.RemoveAll(_ => _.OnGuiCallback == callback);
        }

#if DEBUG_MENU_ENABLED
        void Awake()
        {
            GUIScaler.Initialize();

            _panelWidth = Screen.width / 3;
        }

        void Update()
        {
            if (Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)
            {
                if (Input.GetKeyDown(KeyCode.F8))
                {
                    _isFolded = !_isFolded;
                }
            }
            else
            {
                if (_debugMenuEnabled)
                {
                    var currentTouchCount = Input.touchCount;
                    if (_lastTouchCount < 3 &&
                        currentTouchCount >= 3)
                    {
                        _isFolded = !_isFolded;
                    }

                    _lastTouchCount = currentTouchCount;
                }
            }
        }

        public void SetDebugMenuEnabled(bool enabled)
        {
            _debugMenuEnabled = enabled;
        }

        void OnGUI()
        {
            if (_isFolded)
            {
                return;
            }

            var areaRect = new Rect(0, 0, _panelWidth, Screen.height);

            var oldGuiColor = GUI.color;
            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.DrawTexture(areaRect, Texture2D.whiteTexture);
            GUI.color = oldGuiColor;

            areaRect.width /= GUIScaler.Scale;
            areaRect.height /= GUIScaler.Scale;

            GUILayout.BeginArea(areaRect);

            GUIScaler.Begin();
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            foreach (var each in _tabs)
            {
                each.IsFolded = GUILayout.Toggle(each.IsFolded, each.Name);

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Space(20 * GUIScaler.Scale);

                    try
                    {
                        if (each.IsFolded)
                        {
                            using (new GUILayout.VerticalScope())
                            {
                                each.OnGuiCallback();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        GUILayout.Label(e.ToString());
                    }
                }
            }

            GUILayout.EndScrollView();
            GUIScaler.End();

            GUILayout.EndArea();
        }
#endif
    }
}