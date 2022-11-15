using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tools.VisualEffects {
    public class AttachedParticleEffect : ParticleEffect {

        public Transform Target { get; private set; }

        public void SetTarget(Transform target, bool resetPosAndRot = true) {
            Target = target;
            transform.SetParent(Target);
            if (resetPosAndRot) {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }

        protected override void OnParticleEffectComplete() {
            base.OnParticleEffectComplete();
            Stop();
        }
        
        public override void Stop() {
            base.Stop();
            transform.SetParent(EffectsHost);
            gameObject.SetActive(false);
        }
    }
}