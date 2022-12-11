using System.Collections;
using System.Collections.Generic;
using Game.Dmg;
using Game.Shooting;
using Tools.VisualEffects;
using UnityEngine;

namespace Game.SeaGameplay {
    public class ShipWeapon : MonoBehaviour {
        [SerializeField]
        private Transform _FirePoint;
        [SerializeField]
        private string _ProjectileName;
        [SerializeField]
        private string _FireEffectName;
        [SerializeField]
        private float _ProjectileDamage;
        [SerializeField]
        private float _ProjectileSpeed;
        [SerializeField]
        private float _ProjectileLifetime;
        [SerializeField]
        private float _ProjectileGravity;
        [SerializeField]
        private float _ProjectileXDispersionAngle;
        [SerializeField]
        private float _ProjectileYDispersionAngle;
        [SerializeField]
        private Vector2 _RandomShotDelay;

        public ShipWeaponController Owner { get; private set; }

        public Transform FirePoint => _FirePoint;
        
        public void Setup(ShipWeaponController owner) {
            Owner = owner;
        }
        
        public void Fire() {
            StartCoroutine(FireRoutine());
        }

        private IEnumerator FireRoutine() {
            var delay = Random.Range(_RandomShotDelay.x, _RandomShotDelay.y);
            if (delay > 0)
                yield return new WaitForSeconds(delay);
            
            var projectile = VisualEffect.GetEffect<BulletProjectile>(_ProjectileName);

            var rotationEuler = _FirePoint.rotation.eulerAngles;
            rotationEuler = new Vector3(rotationEuler.x + GetDispersionAngle(_ProjectileXDispersionAngle),
                rotationEuler.y + GetDispersionAngle(_ProjectileYDispersionAngle), rotationEuler.z);
            
            var data = new BulletProjectileData {
                Position = _FirePoint.position,
                Rotation = Quaternion.Euler(rotationEuler),
                LifeTime = _ProjectileLifetime,
                BirthTime = Time.time,
                Speed = _ProjectileSpeed,
                Gravity = _ProjectileGravity,
                Damage = new Damage {
                    ReceiverId = null,
                    CasterId = Owner.Ship.DamageCasterId,
                    Amount = _ProjectileDamage, 
                },
            };
            projectile.Setup(data);
            projectile.Play();
            PlayEffect();
        }

        private void PlayEffect() {
            if (!string.IsNullOrEmpty(_FireEffectName)) {
                var effect = VisualEffect.GetEffect<AttachedParticleEffect>(_FireEffectName);
                effect.transform.position = _FirePoint.position;
                effect.transform.rotation = _FirePoint.rotation;
                effect.SetTarget(_FirePoint);
                effect.Play();
            }
        }

        private float GetDispersionAngle(float dispersion) {
            return Random.Range(-dispersion / 2, dispersion / 2);
        }
    }
}
