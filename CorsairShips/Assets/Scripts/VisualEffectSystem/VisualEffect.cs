using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools.VisualEffects {
    public abstract class VisualEffect : MonoBehaviour {

        private static List<VisualEffect> _ExistingResources;
        private static readonly Dictionary<string, VisualEffect> _ResourcesCache = new Dictionary<string, VisualEffect>();
        private static readonly Dictionary<string, List<VisualEffect>> _EffectsCache = new Dictionary<string, List<VisualEffect>>();

        public static Transform EffectsHost {
            get {
                if (_EffectsHost == null)
                    _EffectsHost = new GameObject("Effects").transform;
                return _EffectsHost;
            }
        }
        private static Transform _EffectsHost;

        private void Register() {
            if (!_EffectsCache.ContainsKey(this.name)) {
                _EffectsCache.Add(this.name, new List<VisualEffect>());
            }
            _EffectsCache[this.name].Add(this);
        }

        protected virtual void OnDestroy() {
            if(_EffectsCache.ContainsKey(this.name))
                _EffectsCache[this.name].Remove(this);
        }

        public virtual void Play() {
            this.gameObject.SetActive(true);
            StartCoroutine(PlayTask());
        }

        public virtual void Stop() {
            StopCoroutine(PlayTask());
        }
        
        protected abstract IEnumerator PlayTask();

        public static T GetEffect<T>(string name) where T : VisualEffect {
            T result = null;
            if (_EffectsCache.ContainsKey(name)) {
                result = _EffectsCache[name].FirstOrDefault(_ => !_.gameObject.activeSelf) as T;
            }
            if (result == null) {
                result = Instantiate(GetEffectResource<T>(name), EffectsHost, false);
                result.name = name;
                result.gameObject.SetActive(false);
                result.Register();
            }
            return result;
        }

        public static void PrewarmSystem() {
            LoadEffectResources();
            foreach (var resource in _ExistingResources) {
                GetEffect<VisualEffect>(resource.name);
            }
        }

        private static void LoadEffectResources() {
            if (_ExistingResources == null) {
                _ExistingResources = Resources.LoadAll<VisualEffect>("Prefabs/Effects").ToList();
            }
        }

        private static VisualEffect GetEffectResource(string name) {
            LoadEffectResources();
            if (!_ResourcesCache.ContainsKey(name)) {
                _ResourcesCache.Add(name, _ExistingResources.FirstOrDefault(_ => _.name == name));
            }
            return _ResourcesCache[name];
        }

        private static T GetEffectResource<T>(string name) where T : VisualEffect {
            return GetEffectResource(name) as T;
        }
    }
}