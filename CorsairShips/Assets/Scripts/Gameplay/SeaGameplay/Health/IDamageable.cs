using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Dmg {
    public interface IDamageable {
        byte DamageableId { get; }
        float MaxHealth { get; }
        float Health { get; }
        float NormalizedHealth { get; }
        bool Dead { get; }
        Collider Collider { get; }
        void ApplyDamage(ClientDamage damage);
    }
}
