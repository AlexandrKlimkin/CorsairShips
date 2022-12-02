using System;
using System.Collections;
using System.Collections.Generic;
using Game.Dmg;
using UnityDI;
using UnityEngine;

namespace Game.SeaGameplay {
    public class ShipWeaponController : MonoBehaviour, IDamageCaster {
        [Dependency]
        private readonly DamageService _DamageService;
        
        [SerializeField]
        private float WeaponsAngle;
        [SerializeField]
        private float Cooldown;
        [SerializeField]
        private List<ShipWeapon> _Weapons;

        public Ship Ship { get; private set; }

        private float _LastShotTime = float.NegativeInfinity;

        public float NormilizedCD => Mathf.Clamp01((Time.time - _LastShotTime) / Cooldown);
        
        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
            Ship = GetComponent<Ship>();
        }

        public void Setup() {
            Ship.ShipModel.WeaponsContainer.GetComponentsInChildren(_Weapons);
            
            var weaponsRot = Quaternion.Euler(0, 0, WeaponsAngle);
            _Weapons.ForEach(_ => {
                _.Setup(this);
                _.transform.localRotation = weaponsRot;
            });
            _DamageService.RegisterCaster(this);
        }

        public void TryFire() {
            if(NormilizedCD >= 1)
                Fire();
        }

        private void Fire() {
            _Weapons.ForEach(_ => _.Fire());
            _LastShotTime = Time.time;
        }
        public byte DamageCasterId => Ship.ShipData.ShipId;
    }
}
