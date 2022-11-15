using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityDI;

namespace UI.Markers {

    public class MarkerCanvas : MonoBehaviour {

        [Dependency]
        private MarkerService _MarkerService;

        private HashSet<MarkerProvider> _MarkerProviders = new HashSet<MarkerProvider>();

        private List<MarkerWidget> _MarkerWidgets = new List<MarkerWidget>();

        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
        }

        private void OnEnable() {
            _MarkerService?.RegisterCanvas(this);
        }
        private void OnDisable() {
            _MarkerService?.UnregisterCanvas(this);
        }

        public virtual bool CheckCompatability(MarkerProvider provider) {
            return true;
        }

        public void TryRegisterProviders(IEnumerable<MarkerProvider> providers) {
            providers.ForEach(_ => TryRegisterProvider(_));
        }

        public void TryRegisterProvider(MarkerProvider provider) {
            if (!CheckCompatability(provider))
                return;
            if (!_MarkerProviders.Add(provider))
                return;
            provider.OnVisibilityChanged += OnProviderVisibilityChanged;
            OnProviderVisibilityChanged(provider);
        }

        public void UnregisterProvider(MarkerProvider provider) {
            _MarkerProviders.Remove(provider);
            provider.OnVisibilityChanged -= OnProviderVisibilityChanged;
        }

        private void OnProviderVisibilityChanged(MarkerProvider provider) {
            if (provider.Visible) {
                GetMarker(provider.RequiredMarkerType).AssignProvider(provider);
            }
        }

        private MarkerWidget GetMarker(Type t) {
            var marker = _MarkerWidgets.FirstOrDefault(_ => (!_.gameObject.activeSelf) && _.GetType() == t);
            if (marker == null) {
                marker = AddMarker(t);
            }
            return marker;
        }

        private MarkerWidget AddMarker(Type type) {
            var markerWidget = Instantiate(MarkerResourcesCache.GetMarker(type));
            markerWidget.transform.SetParent(this.transform);
            markerWidget.transform.localScale = Vector3.one;
            _MarkerWidgets.Add(markerWidget);
            return markerWidget;
        }
    }
}