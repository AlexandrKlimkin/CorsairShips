using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools.VisualEffects {
    public class ParticleEffect : VisualEffect {

        protected ParticleSystem _ParticleSystem;

        protected void Awake() {
            _ParticleSystem = this.GetComponentInChildren<ParticleSystem>();
        }

        protected override IEnumerator PlayTask() {
            _ParticleSystem.Play(true);
            OnParticleEffectStarted();
            yield return new WaitWhile(() => _ParticleSystem.IsAlive(true));
            OnParticleEffectComplete();
            this.gameObject.SetActive(false);
        }

        protected virtual void OnParticleEffectStarted() {

        }

        protected virtual void OnParticleEffectComplete() {

        }
    }
}