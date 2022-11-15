using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tools.VisualEffects {
    public class TrailEffect : VisualEffect {

        private List<TrailRenderer> _Trails;
        private Transform _Target;
        private Vector3 _LastTargetPosition;
        private Quaternion _LastTargetRotation;
        private bool _Attached;
        private bool _LastAttached;
        private float _Lifetime;

        private bool DetachedRecently => !_Attached && _LastAttached;

        void Awake() {
            _Trails = new List<TrailRenderer>();
            this.GetComponentsInChildren(_Trails);
            _Trails.ForEach(_ => DisableTrail(_));
            _Lifetime = _Trails.Max(_ => _.time);
        }

        private void Update() {
            UpdatePosition();
            _LastAttached = _Attached;
        }

        public void Attach(Transform target) {
            _Target = target;
            _Attached = true;
            _LastTargetPosition = new Vector3();
            _LastTargetRotation = new Quaternion();
        }

        public void Detach() {
            _LastTargetPosition = _Target.position;
            _LastTargetRotation = _Target.rotation;
            _Target = null;
            _Attached = false;
        }

        protected override IEnumerator PlayTask() {
            UpdatePosition();
            yield return null;
            _Trails.ForEach(_ => EnableTrail(_));
            yield return new WaitUntil(() => _Attached == false);
            yield return new WaitForSeconds(_Lifetime);
            _Trails.ForEach(_ => DisableTrail(_));
            this.gameObject.SetActive(false);
        }

        private void UpdatePosition() {
            if (_Attached)
                this.transform.SetPositionAndRotation(_Target.position, _Target.rotation);
            else if (DetachedRecently)
                this.transform.SetPositionAndRotation(_LastTargetPosition, _LastTargetRotation);
        }

        private void DisableTrail(TrailRenderer trail) {
            trail.emitting = false;
        }

        private void EnableTrail(TrailRenderer trail) {
            trail.Clear();
            trail.emitting = true;
        }
    }
}