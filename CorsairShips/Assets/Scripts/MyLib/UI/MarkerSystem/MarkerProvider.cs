using UnityEngine;
using System;
using UnityDI;

namespace UI.Markers {

    public abstract class MarkerProvider : MonoBehaviour {

        public event Action<MarkerProvider> OnVisibilityChanged;

        public bool Visible {
            get {
                return _Visible;
            }
            set {
                if (_Visible != value) {
                    _Visible = value;
                    if (OnVisibilityChanged != null)
                        OnVisibilityChanged(this);
                }
            }
        }
        private bool _Visible;

        private static MarkerService _CachedMarkerService;

        public static MarkerService GetCachedMarkerService() {
            if (_CachedMarkerService == null)
                _CachedMarkerService = ContainerHolder.Container.Resolve<MarkerService>();
            return _CachedMarkerService;
        }

        protected virtual void OnEnable() {
            GetCachedMarkerService().RegisterProvider(this);
        }

        protected virtual void OnDisable() {
            GetCachedMarkerService().UnregisterProvider(this);
            Visible = false;
        }

        public virtual void UpdateProvider() {
            Visible = GetVisibility();
        }

        public abstract Type RequiredMarkerType { get; }

        public abstract MarkerData GetMarkerData();

        public abstract bool GetVisibility();
    }

    public abstract class MarkerProvider<W, D> : MarkerProvider where W : MarkerWidget<D> where D : MarkerData, new() {      

        public sealed override Type RequiredMarkerType {
            get {
                return typeof(W);
            }
        }
        D Data = new D();

        protected virtual void RefreshData(D data) {
            data.WorldPosition = this.transform.position;
        }

        public sealed override MarkerData GetMarkerData() {
            RefreshData(Data);
            return Data;
        }
    }
}