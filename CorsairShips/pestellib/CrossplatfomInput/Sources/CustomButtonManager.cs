using System.Collections.Generic;
using PestelLib.UI;
using UnityEngine;
using UnityDI;

namespace PestelLib.CrossplatfomInput
{
	public class CustomButtonManager : MonoBehaviour
	{
	    [Dependency] private Gui _gui;

        public HashSet<GameObject> AllRaycasts = new HashSet<GameObject>();
        public HashSet<RectTransform> Buttons = new HashSet<RectTransform>();

	    private Camera _uiCamera;

	    void Start()
	    {
            ContainerHolder.Container.BuildUp(this);
	        _uiCamera = _gui.UiCamera;
	    }

        void Update()
        {
            AllRaycasts.Clear();

            if(_gui.AnyVisibleDialog) return;

#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButton(0))
            {
                TestPoint(Input.mousePosition);
            }
#endif

            for (var i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);

                var pos = touch.position;
                TestPoint(pos);
            }
        }

        private void TestPoint(Vector2 pos)
        {
            foreach (var customButton in Buttons)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(customButton, pos, _uiCamera))
                {
                    if (!AllRaycasts.Contains(customButton.gameObject))
                    {
                        AllRaycasts.Add(customButton.gameObject);
                    }
                }
            }
        }
	}
}