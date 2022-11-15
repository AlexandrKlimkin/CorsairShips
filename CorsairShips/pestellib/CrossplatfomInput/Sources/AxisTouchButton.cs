using System.Collections.Generic;
using PestelLib.CrossplatfomInput;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace UnityStandardAssets.CrossPlatformInput
{
	public class AxisTouchButton : MonoBehaviour
	{
        public static Dictionary<string, bool> ButtonStates = new Dictionary<string, bool>();
		public UnityEvent OnPress;

	    [Dependency] private CustomButtonManager _customButtonManager;

        [Header("Keyboard setup")]
	    [SerializeField] private KeyCode[] _standaloneButtons;
	    private RectTransform _rectTransform;

        
        [Header("Custom Button")]
        [SerializeField] private CanvasGroup _parentCanvasGroup;
	    [SerializeField] private bool _inheritDisableFromParent = true;
	    [SerializeField] private Button _button;

	    private bool _keyPressed;
	    private bool _touchPressed;

	    string _name;

        void Awake()
	    {
	        ContainerHolder.Container.BuildUp(this);
	        _name = name;
	        ButtonStates[_name] = false;
            _rectTransform = GetComponent<RectTransform>();
            _customButtonManager.Buttons.Add(_rectTransform);
        }

	    void OnDestroy()
		{
	        _customButtonManager.Buttons.Remove(_rectTransform);
        }

		void OnEnable()
		{
		    _keyPressed = false;
		    _touchPressed = false;
		    ButtonStates[_name] = false;
		}

		void OnDisable()
	    {
	        _keyPressed = false;
	        _touchPressed = false;
            ButtonStates[_name] = false;
	    }

	    void Update()
	    {
            UpdateKeyboardInput();
	        UpdateCustomButton();
	        bool wasPressed = ButtonStates[_name];
	        ButtonStates[_name] = _keyPressed || _touchPressed;
	        if (!wasPressed && ButtonStates[_name])
			{
				OnPress.Invoke();
			}
	    }

	    private void UpdateKeyboardInput()
	    {
	        for (int i = 0; i < _standaloneButtons.Length; i++)
	        {
	            if (Input.GetKeyDown(_standaloneButtons[i]))
	            {
	                _keyPressed = true;
	            }

	            if (Input.GetKeyUp(_standaloneButtons[i]))
	            {
	                _keyPressed = false;
	            }
	        }
	    }

	    void UpdateCustomButton()
	    {
	        var buttonInteractable = _button == null || _button.interactable;

	        bool isDisabledByParent = (_parentCanvasGroup != null && !_parentCanvasGroup.interactable) && _inheritDisableFromParent;
	        if (!buttonInteractable || isDisabledByParent)
	        {
	            _touchPressed = false;
	            return;
	        }

	        _touchPressed = _customButtonManager.AllRaycasts.Contains(gameObject);
	    }
	}
}