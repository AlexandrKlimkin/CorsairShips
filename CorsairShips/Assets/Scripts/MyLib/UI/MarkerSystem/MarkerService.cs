using System.Collections;
using System.Linq;
using System.Collections.Generic;
using PestelLib.Utils;
using UnityDI;
using UTPLib.Services;

namespace UI.Markers {

    public class MarkerService : ILoadableService, IUnloadableService {

        [Dependency]
        private UnityEventsProvider _UnityEventProvider;

        private List<MarkerProvider> _MarkerProviders = new List<MarkerProvider>();
        private HashSet<MarkerCanvas> _MarkerCanvases = new HashSet<MarkerCanvas>();

        // TODO
        //public void ResetWidgets() {
        //    Instance._MarkerWidgets.ForEach(_ => DestroyImmediate(_.gameObject));
        //    Instance._MarkerWidgets = new List<MarkerWidget>();
        //    _MarkerProviders.ForEach(_ => OnProviderVisibilityChanged(_));
        //}

        public void RegisterCanvas(MarkerCanvas canvas) {
            if (_MarkerCanvases.Contains(canvas))
                return;
            _MarkerCanvases.Add(canvas);
            LinkProvidersToCanvas(canvas);
        }

        public void UnregisterCanvas(MarkerCanvas canvas) {
            _MarkerCanvases.Remove(canvas);
        }

        private void LinkProvidersToCanvas(MarkerCanvas canvas) {
            //var providers = _MarkerProviders.Where(_ => canvas.CheckCompatability(_));
            //canvas.TryRegisterProviders(providers);
            canvas.TryRegisterProviders(_MarkerProviders);
        }

        public void RegisterProvider(MarkerProvider provider) {
            if (_MarkerProviders.Contains(provider))
                return;
            _MarkerProviders.Add(provider);
            LinkProvider(provider);
        }

        public void UnregisterProvider(MarkerProvider provider) {
            _MarkerProviders.Remove(provider);
        }

        private void LinkProvider(MarkerProvider provider) {
            _MarkerCanvases.ForEach(_ => _.TryRegisterProvider(provider));
        }

        private IEnumerator UpdateMarkerProviders() {          
            while (true) {
                if (_MarkerCanvases.Count(_ => _.isActiveAndEnabled) > 0) {
                    for (var i = 0; i < _MarkerProviders.Count; i++) {
                        _MarkerProviders[i].UpdateProvider();
                        if (i % 50 == 0) { // Hack if we have too many markerproviders
                            yield return null;
                        }
                    }
                }
                yield return null;
            }
        }

        public void Load() {
            _UnityEventProvider.StartCoroutine(UpdateMarkerProviders());
        }

        public void Unload() {
            _UnityEventProvider.StopCoroutine(UpdateMarkerProviders());
        }
    }
}