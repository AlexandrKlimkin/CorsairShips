using UnityDI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PestelLib.CrossplatfomInput
{ 
	public class CustomButton : MonoBehaviour
	{
	    [Dependency] private CustomButtonManager _customButtonManager;

        public UnityEvent OnPress;
        public UnityEvent OnRelease;

	    private bool _pressed;
	    private Button _button;
	    private RectTransform _rectTransform;
	    [SerializeField] private CanvasGroup _parentCanvasGroup;
	    [SerializeField] private bool _inheritDisableFromParent = true;

	    void Awake()
	    {
	        ContainerHolder.Container.BuildUp(this);
	        _button = GetComponent<Button>();
	        _rectTransform = GetComponent<RectTransform>();
	    }

	    void OnEnable()
	    {
            _customButtonManager.Buttons.Add(_rectTransform);
	    }

	    void OnDisable()
	    {
            if (_pressed)
                OnRelease.Invoke();

            _pressed = false;
            _customButtonManager.Buttons.Remove(_rectTransform);
	    }

		void Update ()
		{
		    var buttonInteractable = _button == null || _button.interactable;

            bool isDisabledByParent = (_parentCanvasGroup != null && !_parentCanvasGroup.interactable) && _inheritDisableFromParent;
            if (!buttonInteractable || isDisabledByParent)
		    {
                if (_pressed)
                    OnRelease.Invoke();

		        _pressed = false;
		        return;
		    }

            if (_customButtonManager.AllRaycasts.Contains(gameObject)) {
	            if (!_pressed)
	            {
	                _pressed = true;
	                OnPress.Invoke();
	            }
	        }
	        else
	        {
	            if (_pressed)
	            {
	                _pressed = false;
	                OnRelease.Invoke();
	            }
	        }
		}
	}
}