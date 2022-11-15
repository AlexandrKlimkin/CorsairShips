using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugMenu
{
    public class DebugMenuTest : MonoBehaviour
    {
        [SerializeField]
        private Texture2D _texture;

        private string _text1;

        // Use this for initialization
        void Start()
        {
            DebugMenuController.Instance.AddMenuTab("Menu Tab 1", MenuTab1);
            DebugMenuController.Instance.AddMenuTab("Menu Tab 2", MenuTab2);
            DebugMenuController.Instance.AddMenuTab("Menu Tab 3", MenuTab3);
            DebugMenuController.Instance.AddMenuTab("Menu Tab 4", MenuTab4);
        }

        void OnDestroy()
        {
            DebugMenuController.Instance.RemoveMenuTab(MenuTab1);
            DebugMenuController.Instance.RemoveMenuTab(MenuTab2);
            DebugMenuController.Instance.RemoveMenuTab(MenuTab3);
            DebugMenuController.Instance.RemoveMenuTab(MenuTab4);
        }

        private void MenuTab1()
        {
            _text1 = GUILayout.TextField(_text1, GUILayout.ExpandHeight(false));
        }

        private void MenuTab2()
        {
            throw new Exception("Test message!");
        }

        private void MenuTab3()
        {
            var textureRect = GUILayoutUtility.GetRect(100, 100, GUILayout.ExpandWidth(false));
            GUI.DrawTexture(textureRect, _texture);
        }

        private void MenuTab4()
        {
            for (int i = 0; i < 100; i++)
            {
                GUILayout.Label(i.ToString());
            }
        }
    }
}