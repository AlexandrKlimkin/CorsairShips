using UnityEngine;

namespace PestelLib.UI
{
    public class CanvasGroupFader : MonoBehaviour
    {
        public CanvasGroup _canvasGroup;
        [SerializeField]
        protected float _smoothTime = 1f;

        [SerializeField] protected bool _useUnscaledDeltaTime = false;

        protected float _targetAlpha;
        protected float _currentSpeed;

        public void SetActive(bool active)
        {
            _targetAlpha = active ? 1 : 0;
            if (active && !gameObject.activeSelf)
            {
                CurrentAlpha = 0;
                SetPopupBackgroundState(true);
            }
        }

        public void SetActiveNow(bool active)
        {
            _targetAlpha = active ? 1 : 0;
            CurrentAlpha = active ? 1 : 0;
        }

        virtual protected void SetPopupBackgroundState(bool b)
        {
            gameObject.SetActive(b);
        }

        protected virtual float CurrentAlpha
        {
            get { return _canvasGroup.alpha; }
            set { _canvasGroup.alpha = value; }
        }

        public virtual void Update()
        {
            if (CurrentAlpha == 0 && _targetAlpha == 0)
            {
                SetPopupBackgroundState(false);
            }

            if (_useUnscaledDeltaTime)
            {
                CurrentAlpha = Mathf.MoveTowards(CurrentAlpha, _targetAlpha, Time.unscaledDeltaTime / _smoothTime);
            }
            else
            {
                CurrentAlpha = Mathf.MoveTowards(CurrentAlpha, _targetAlpha, Time.deltaTime / _smoothTime);
            }
        }
    }
}