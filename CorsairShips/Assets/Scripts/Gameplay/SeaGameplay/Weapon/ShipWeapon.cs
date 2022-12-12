using System.Collections;
using System.Collections.Generic;
using Game.Dmg;
using Game.Shooting;
using Stats;
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
        
        
        [Header("Stats from configs")]
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

        private StatsController StatsController => Owner.Ship.StatsController;
        
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

            _ProjectileDamage = StatsController.GetBuffedStatValue<float>(StatId.ProjectileDamage);
            _ProjectileSpeed = StatsController.GetBuffedStatValue<float>(StatId.ProjectileSpeed);
            _ProjectileLifetime = StatsController.GetBuffedStatValue<float>(StatId.ProjectileLifetime);
            _ProjectileGravity = StatsController.GetBuffedStatValue<float>(StatId.ProjectileGravity);
            _ProjectileXDispersionAngle = StatsController.GetBuffedStatValue<float>(StatId.ProjectileXDispersionAngle);
            _ProjectileYDispersionAngle = StatsController.GetBuffedStatValue<float>(StatId.ProjectileYDispersionAngle);
            
            var delayMin = StatsController.GetBuffedStatValue<float>(StatId.RandomShotDelayMin);
            var delayMax = StatsController.GetBuffedStatValue<float>(StatId.RandomShoDelayMax);
            _RandomShotDelay = new Vector2(delayMin, delayMax);

            var scaleFactor = StatsController.GetBuffedStatValue<float>(StatId.ProjectileScaleFactor);


            var data = new BulletProjectileData {
                Position = _FirePoint.position,
                Rotation = Quaternion.Euler(rotationEuler),
                Scale = Vector3.one * scaleFactor,
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
