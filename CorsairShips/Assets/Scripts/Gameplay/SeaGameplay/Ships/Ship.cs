using System;
using System.Collections;
using System.Collections.Generic;
using Game.Health;
using PestelLib.SharedLogic.Modules;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.SeaGameplay {
    public class Ship : MonoBehaviour, IDamageable {

        [SerializeField]
        private Transform _ModelContainer;
        
        public ShipMovementController MovementController { get; private set; }
        public ShipWeaponController WeaponController { get; private set; }
        public ShipModelController ShipModel { get; private set; }
        public Transform ModelContainer => _ModelContainer;
        
        public ShipData ShipData { get; private set; }
        public ShipDef ShipDef { get; private set; }
        
        public float MaxHp;
        public float DieImpulse;
        public float DrownDelay;
        public float DestroyTime;
        
        private void Awake() {
            MovementController = GetComponent<ShipMovementController>();
            WeaponController = GetComponent<ShipWeaponController>();
            Collider = GetComponent<Collider>();
        }

        public void Setup(ShipData shipData, ShipDef shipDef, ShipModelController model) {
            ShipData = shipData;
            ShipDef = shipDef;
            ShipModel = model;
            MaxHealth = MaxHp;
            Health = MaxHealth;
            MovementController.Setup();
            WeaponController.Setup();
        }
        
        #region IDamageable
        public float MaxHealth { get; private set; }
        public float Health { get; private set; }
        public float NormalizedHealth => Health / MaxHealth;
        public bool Dead { get; private set; }
        public Collider Collider { get; private set; }
        public void ApplyDamage(Damage damage) {
            Health -= damage.Amount;
            Health = Mathf.Clamp(Health, 0, MaxHealth);
            // Debug.LogError("Taking damage");
            if(Health == 0)
                Die();
        }
        #endregion

        #region Dying
        [Button]
        private void Die() {
            if(Dead)
                return;
            Dead = true;

            StartCoroutine(DrownRoutine());
            ShipModel.PlayDieExplosions();
            StartCoroutine(DestroyRoutine());
        }

        private IEnumerator DrownRoutine() {
            yield return new WaitForSeconds(DrownDelay);
            
            ShipModel.SetTrailsEnabled(false);
            
            MovementController.Rigidbody.constraints = RigidbodyConstraints.None;
            var boundsMin = Collider.bounds.min;
            var boundsMax = Collider.bounds.max;

            var randX = Random.Range(boundsMin.x, boundsMax.x);
            var randZ = Random.Range(boundsMin.z, boundsMax.z);
            var randY = Random.Range(boundsMin.y, boundsMax.y);
            
            var randInsideBounds = new Vector3(randX, randY, randZ);
            
            MovementController.Rigidbody.centerOfMass = new Vector3(randX, randY, randZ);
            MovementController.Rigidbody.AddForceAtPosition(Random.onUnitSphere * DieImpulse, transform.position + randInsideBounds, ForceMode.Impulse);

        }

        private IEnumerator DestroyRoutine() {
            yield return new WaitForSeconds(DestroyTime);
            Destroy(gameObject);
        }
        #endregion
    }
}
