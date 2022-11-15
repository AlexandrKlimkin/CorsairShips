using System;
using System.Collections;
using System.Collections.Generic;
using Game.Health;
using UnityEngine;

namespace Game.SeaGameplay {
    public class Ship : MonoBehaviour, IDamageable {
        public ShipMovementController MovementController { get; private set; }
        public ShipWeaponController WeaponController { get; private set; }

        private void Awake() {
            MovementController = GetComponent<ShipMovementController>();
            WeaponController = GetComponent<ShipWeaponController>();
            Collider = GetComponent<Collider>();
        }
        
        #region IDamageable
        
        public float MaxHealth { get; private set; }
        public float Health { get; private set; }
        public float NormalizedHealth { get; private set; }
        public bool Dead { get; private set; }
        public Collider Collider { get; private set; }
        public void ApplyDamage(Damage damage) {
            Debug.LogError("Taking damage");
        }
        
        #endregion
    }
}
