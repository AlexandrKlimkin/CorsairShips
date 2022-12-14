using System;
using System.Collections;
using System.Collections.Generic;
using Game.Dmg;
using Game.SeaGameplay.Data;
using PestelLib.SharedLogic.Modules;
using Sirenix.OdinInspector;
using Stats;
using UnityDI;
using UnityEngine;
using UTPLib.SignalBus;
using Random = UnityEngine.Random;

namespace Game.SeaGameplay {
    public class Ship : MonoBehaviour, IDamageable, IDamageCaster  {
        [Dependency]
        private readonly SignalBus _SignalBus;
        [Dependency]
        private readonly DamageService _DamageService;
        [Dependency]
        private readonly ILocalShipProvider _LocalShipProvider;
        [Dependency]
        private readonly StatsService _StatsService;

        [SerializeField]
        private Transform _ModelContainer;
        
        public ShipMovementController MovementController { get; private set; }
        public ShipWeaponController WeaponController { get; private set; }
        public ShipModelController ShipModel { get; private set; }
        public StatsController StatsController { get; private set; }
        public DamageBuffer DamageBuffer { get; private set; }
        
        public Transform ModelContainer => _ModelContainer;
        public ShipData ShipData { get; private set; }
        public ShipDef ShipDef { get; private set; }
        public Vector3 Position => transform.position;
        
        // public float DieImpulse;
        public float DestroyTime;

        public event Action<Ship> OnDie;
        public event Action<Ship> OnShipDestroy;
        public event Action<ClientDamage> OnTakeDamage;
        
        private static List<Ship> _Ships = new List<Ship>();
        public static IReadOnlyList<Ship> Ships => _Ships;
        public bool IsLocalPlayerShip => this == _LocalShipProvider.LocalShip;
        public bool IsAlive => !Dead;
        
        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
            MovementController = GetComponent<ShipMovementController>();
            WeaponController = GetComponent<ShipWeaponController>();
            DamageBuffer = GetComponent<DamageBuffer>();
        }

        public void Setup(ShipData shipData, ShipDef shipDef, ShipModelController model) {
            ShipData = shipData;
            ShipDef = shipDef;
            ShipModel = model;

            Collider = ShipModel.GetComponentInChildren<Collider>();
            StatsController = _StatsService.CreateStatsController(new StatsDescriptor(StatsHolderType.Ship, ShipDef.ConfigId, 0));
            
            MovementController.Setup();
            WeaponController.Setup();
            ShipModel.BatchView();
            
            _Ships.Add(this);

            MaxHealth = StatsController.GetRawStatValue<float>(StatId.MaxHealth);
            Health = MaxHealth;
            
            DamageBuffer.Initialize(this, 5f);
            
            _DamageService.RegisterDamageable(this);
            _DamageService.RegisterCaster(this);
        }
        
        #region IDamageable

        public byte DamageableId => ShipData.ShipId;
        public float MaxHealth { get; private set; }
        public float Health { get; private set; }
        public float NormalizedHealth => Health / MaxHealth;
        public bool Dead { get; private set; }
        public Collider Collider { get; private set; }
        public void ApplyDamage(ClientDamage damage) {
            Health -= damage.Damage.Amount;
            Health = Mathf.Clamp(Health, 0, MaxHealth);
            // Debug.LogError("Taking damage");
            OnTakeDamage?.Invoke(damage);
            if(Health == 0)
                Die(damage);
        }
        #endregion

        #region IDamageCaster
        public byte DamageCasterId => ShipData.ShipId;
        #endregion
        
        #region Dying

        [Button]
        private void DieForce() {
            Die(new ClientDamage());
        }
        
        private void Die(ClientDamage damage) {
            if(Dead)
                return;
            Dead = true;

            MovementController.Gaz = 0;
            
            StartCoroutine(DrownRoutine());
            ShipModel.PlayDieExplosions();
            StartCoroutine(DestroyRoutine());
            _SignalBus.FireSignal(new ShipDieSignal {
                Ship = this,
                Damage = damage,
            });
            OnDie?.Invoke(this);
        }

        private IEnumerator DrownRoutine() {
            yield return new WaitForSeconds(ShipModel.DrownDelay);
            
            ShipModel.SetTrailsEnabled(false);
            
            MovementController.Rigidbody.constraints = RigidbodyConstraints.None;
            // var boundsMin = Collider.bounds.min;
            // var boundsMax = Collider.bounds.max;

            // var randX = Random.Range(boundsMin.x, boundsMax.x);
            // var randZ = Random.Range(boundsMin.z, boundsMax.z);
            // var randY = Random.Range(boundsMin.y, boundsMax.y);
            
            // var randInsideBounds = new Vector3(randX, randY, randZ);
            
            // MovementController.Rigidbody.centerOfMass = new Vector3(randX, randY, randZ);
            // MovementController.Rigidbody.AddForceAtPosition(Random.onUnitSphere * DieImpulse, transform.position + randInsideBounds, ForceMode.Impulse);
        }

        private IEnumerator DestroyRoutine() {
            yield return new WaitForSeconds(DestroyTime);
            Destroy(gameObject);
        }
        #endregion

        private void OnDestroy() {
            _Ships.Remove(this);
            OnShipDestroy?.Invoke(this);
            _DamageService?.UnregisterCaster(this);
            _DamageService?.UnregisterDamageable(this);
        }
    }
}
