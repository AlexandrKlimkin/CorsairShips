using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace UI.Markers {

    public abstract class MarkerWidget : UIBehaviour {
        public bool Visible { get; protected set; }

        public virtual bool ForceSingle {
            get {
                return false;
            }
        }

        protected RectTransform RectTransform { get; private set; }

        private MarkerProvider _MarkerProvider;
        private Animator _Animator;
        private Coroutine _CurrentDisableCor;
        private MarkerData _CachedMarkerData;

        // protected Camera _Camera;
        protected Rect _ParentRect;
        
        protected override void Awake() {
            base.Awake();
            RectTransform = (RectTransform)this.transform;
            _Animator = this.GetComponent<Animator>();
            // _Camera = Camera.main;
        }

        protected override void Start() {
            base.Start();
            _ParentRect = ((RectTransform)RectTransform.parent).rect;
        }

        public virtual void LateUpdate() {
            if (_MarkerProvider != null) {
                _CachedMarkerData = _MarkerProvider.GetMarkerData();
            }
            else {
                Hide();
            }

            UpdateMarker(_CachedMarkerData);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _MarkerProvider.OnVisibilityChanged -= OnProviderVisibilityChanged;
        }

        public virtual void UpdateMarker(MarkerData data) {
            RectTransform.localPosition = TransformPosition(data.WorldPosition);
        }

        public void AssignProvider(MarkerProvider provider) {
            Debug.Assert(provider != null);
            if (_MarkerProvider != null)
                _MarkerProvider.OnVisibilityChanged -= OnProviderVisibilityChanged;

            _MarkerProvider = provider;
            _MarkerProvider.OnVisibilityChanged += OnProviderVisibilityChanged;
            Show();
        }

        protected void OnProviderVisibilityChanged(MarkerProvider provider) {
            if (!provider.Visible) {
                provider.OnVisibilityChanged -= OnProviderVisibilityChanged;
                Hide();
            }
        }

        public void Show() {
            if (!Visible) {
                Visible = true;
                if (_CurrentDisableCor != null)
                    StopCoroutine(_CurrentDisableCor);
                this.gameObject.SetActive(true);
                if (_Animator)
                    _Animator.SetBool("Visible", true);
            }
        }

        public void Hide() {
            if (Visible) {
                Visible = false;
                if (_Animator)
                    _Animator.SetBool("Visible", false);
                if (this.gameObject) {
                    if (this.gameObject.activeInHierarchy && _Animator != null) {
                        _CurrentDisableCor = StartCoroutine(DisableOnAnimationFinished());
                    }
                    else {
                        this.gameObject.SetActive(false);
                    }
                }
            }
        }

        private IEnumerator DisableOnAnimationFinished() {
            yield return null;
            yield return new WaitForSeconds(_Animator.GetCurrentAnimatorStateInfo(0).length);
            this.gameObject.SetActive(false);
            _CurrentDisableCor = null;
        }

        protected virtual Vector2 TransformPosition(Vector3 position) {
            //ToDo: remove camera main, cache camera
            return Vector3.Scale(new Vector3(_ParentRect.width, _ParentRect.height), Camera.main.WorldToViewportPoint(position) - Vector3.one * 0.5f);
        }
    }

    public abstract class MarkerWidget<D> : MarkerWidget where D : MarkerData {
        protected abstract void HandleData(D data);
        public sealed override void UpdateMarker(MarkerData data) {
            base.UpdateMarker(data);
            D curData = data as D;
            if (curData != null) {
                HandleData(curData);
            }
            else {
                Debug.LogError("Widget " + this.GetType() + " can not handle passed data of type: " + data.GetType());
            }
        }
    }
}