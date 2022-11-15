using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Health {
    public interface IDamageable {
        // byte? OwnerId { get; }
        float MaxHealth { get; }
        float Health { get; }
        float NormalizedHealth { get; }
        bool Dead { get; }
        Collider Collider { get; }
        void ApplyDamage(Damage damage);
        // void Kill(Damage damage);
    }
}
