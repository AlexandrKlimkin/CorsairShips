using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Dmg;
using Tools.VisualEffects;
using UnityDI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Shooting {
    public abstract class Projectile<D> : VisualEffect where D : ProjectileDataBase {
        [Dependency]
        protected readonly DamageService _DamageService;
        
        [Serializable]
        public class LayerEffectsPair {
            public string LayerName;
            public List<VisualEffectInfo> EffectInfos;
        }
        public bool ScaleEffects;
        public List<LayerEffectsPair> HitEffects;
        public D Data { get; private set; }
        public bool Initialized { get; private set; }
        public float NormalizedLifeTime => (Time.time - Data.BirthTime) / Data.LifeTime;

        public abstract void Simulate(float time);

        protected bool _Hit;

        public virtual void Setup(D data) {
            ContainerHolder.Container.BuildUp(GetType(),this);
            Data = data;
            transform.position = data.Position;
            transform.rotation = data.Rotation;
            transform.localScale = data.Scale;
            _Hit = false;
            Initialize();
        }

        protected virtual void Initialize() {
            Initialized = true;
        }

        protected override IEnumerator PlayTask() {
            if (!Initialized)
                yield break;
            while (true) {
                Simulate(Time.deltaTime);
                if (NormalizedLifeTime >= 1) {
                    KillProjectile();
                }
                yield return null;
            }
        }

        protected virtual void KillProjectile() {
            this.gameObject.SetActive(false);
            Initialized = false;
        }

        protected virtual void PerformHit(IDamageable damageable, RaycastHit hit, bool killProjectile = true) {
            if (killProjectile)
                KillProjectile();
            _Hit = true;
            Data.Damage.ReceiverId = damageable?.DamageableId;
            _DamageService.ApplyDamage(Data.Damage);
            if(hit.collider != null)
                PlayHitEffect(hit.collider.gameObject.layer, hit.point, hit.normal);
        }

        protected virtual void PlayHitEffect(int layer, Vector3 pos, Vector3 upwards) {
            var layerName = LayerMask.LayerToName(layer);
            var pair = HitEffects?.FirstOrDefault(_ => _.LayerName == layerName);
            if (pair == null)
                return;
            if(pair.EffectInfos.Count == 0)
                return;
            var randIndex = Random.Range(0, pair.EffectInfos.Count);
            var effect = GetEffect<ParticleEffect>(pair.EffectInfos[randIndex]);
            effect.transform.position = pos;
            effect.transform.up = upwards;
            if (ScaleEffects) {
                var e_scale = effect.transform.localScale;
                var p_scale = transform.localScale;
                effect.transform.localScale = new Vector3(e_scale.x * p_scale.x, e_scale.y * p_scale.y, e_scale.z * p_scale.z);
            }
            effect.Play();
        }

        // protected virtual void ApplyDamage(IDamageable damageable, Damage dmg) {
        //     _DamageService.ApplyDamage(Data.Damage);
        // }
    }
}