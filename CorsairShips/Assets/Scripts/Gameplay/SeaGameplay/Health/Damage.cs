using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Health {
    public class Damage {
        public float Amount;
        // public byte? InstigatorId;
        public IDamageable Receiver;

        public Damage(IDamageable receiver, float amount) {
            // this.InstigatorId = instigator;
            this.Receiver = receiver;
            this.Amount = amount;
        }
    }

}
