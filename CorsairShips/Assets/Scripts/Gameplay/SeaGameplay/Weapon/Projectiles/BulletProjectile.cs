using System.Collections.Generic;
using Tools.VisualEffects;
using UnityEngine;
using System.Linq;
using Game.Health;

namespace Game.Shooting {
    public class BulletProjectile : Projectile<BulletProjectileData> {
        //public List<string> HitEffectNames;
        public Transform TrailTransformOverride;
        public string TrailName;
        

        protected TrailEffect _Trail;

        // private ContactFilter _Filter = new ContactFilter() { useTriggers = false };

        private Vector3 _Velocity;

        public override void Simulate(float time) {
            var delta = _Velocity * time;
            var targetPosition = transform.position + delta;
            
            //var targetPos = transform.position + transform.forward * Data.Speed * time;
            if (Physics.Linecast(transform.position, targetPosition, out var hit)) {
                PerformHit(hit.transform.GetComponentInParent<IDamageable>(), hit);
            }
            if (targetPosition.y < 0) {
                PerformHit(null, default);
                PlayHitEffect(LayerMask.NameToLayer("Water"), new Vector3(transform.position.x, 0.3f, transform.position.z), Vector3.up);
            }
            this.transform.rotation = Quaternion.LookRotation(_Velocity, Vector3.up);
            _Velocity -= Data.Gravity * Vector3.up * time;
            
            transform.position = hit.transform ? hit.point : targetPosition;
        }

        protected override void Initialize() {
            base.Initialize();
            AttachTrail();
            _Velocity = transform.forward * Data.Speed;
        }

        protected override void PerformHit(IDamageable damageable, RaycastHit hit, bool killProjectile = true) {
            damageable?.Collider?.attachedRigidbody?.AddForceAtPosition(new Vector2(transform.forward.x, transform.forward.y) * Data.Force, transform.position);
            base.PerformHit(damageable, hit, killProjectile);
        }

        protected virtual void AttachTrail() {
            if (string.IsNullOrEmpty(TrailName))
                return;
            _Trail = GetEffect<TrailEffect>(TrailName);
            _Trail.gameObject.SetActive(true);
            var target = TrailTransformOverride ?? this.transform;
            _Trail.Attach(target);
            _Trail.Play();
        }

        protected virtual void DetachTrail() {
            if (_Trail == null)
                return;
            _Trail.Detach();
            _Trail = null;
        }

        protected override void KillProjectile() {
            base.KillProjectile();
            DetachTrail();
        }

    }
}