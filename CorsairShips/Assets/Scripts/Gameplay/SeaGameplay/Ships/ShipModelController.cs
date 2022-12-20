using System;
using System.Collections;
using System.Collections.Generic;
using Game.Rails;
using Menu.Camera;
using Sirenix.OdinInspector;
using Tools.VisualEffects;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.SeaGameplay {

    [Serializable]
    public class RandomEffectInfo {
        public string EffectId;
        public Vector2 RandomScale;
    }
    
    public class ShipModelController : MonoBehaviour, ICameraTarget {
        [SerializeField]
        private Transform _WeaponsContainer;
        [SerializeField]
        private Rail _ViewRail;
        [SerializeField]
        private List<RandomEffectInfo> _DieExplosionEffects;
        [SerializeField]
        private List<Transform> _DieExplosionTransforms;
        [SerializeField]
        private Vector2 _RandomExplosionDelay;
        [SerializeField]
        private float _DrownDelay;
        [Space]
        [SerializeField]
        private List<GameObject> _Trails;
        
        public Transform WeaponsContainer => _WeaponsContainer;
        public Rail ViewRail => _ViewRail;
        public float DrownDelay => _DrownDelay;

        // private void Awake() {
        //     BatchView();
        // }

        public void BatchView() {
            StaticBatchingUtility.Combine(gameObject);
        }

        #region Explosion
        [Button]
        public void PlayDieExplosions() {
            var effectsCount = _DieExplosionEffects.Count;
            if(effectsCount == 0)
                return;
            if(_DieExplosionTransforms.Count == 0)
                return;
            foreach (var t in _DieExplosionTransforms) {
                var delay = Random.Range(_RandomExplosionDelay.x, _RandomExplosionDelay.y);
                var randEffectInfo = _DieExplosionEffects[Random.Range(0, effectsCount)];
                StartCoroutine(PlayExplosionRoutine(randEffectInfo, t, delay));
            }
        }

        private IEnumerator PlayExplosionRoutine(RandomEffectInfo effectInfo, Transform point, float delay) {
            yield return new WaitForSeconds(delay);
            PlayExplosionEffect(effectInfo, point);
        }

        private void PlayExplosionEffect(RandomEffectInfo effectInfo, Transform point) {
            var effect = VisualEffect.GetEffect<ParticleEffect>(effectInfo.EffectId);
            var randScale = Random.Range(effectInfo.RandomScale.x, effectInfo.RandomScale.y);
            var scale = new Vector3(randScale, randScale, randScale);
            effect.transform.position = point.position;
            effect.transform.rotation = point.rotation;
            effect.transform.localScale = scale;
            effect.Play();
        }
        #endregion

        #region Trails

        public void SetTrailsEnabled(bool enabled) {
            _Trails.ForEach(_ => _.SetActive(enabled));
        }

        #endregion

        Transform ICameraTarget.Transform { get; }
        Vector3 ICameraTarget.ViewPoint => ViewRail.MidPoint;
        Vector3 ICameraTarget.GetProjectPoint(Vector3 forward) {
            return ViewRail.Project(forward);
        }
    }
}
