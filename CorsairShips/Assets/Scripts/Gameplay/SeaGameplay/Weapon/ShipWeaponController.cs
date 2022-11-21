using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.SeaGameplay {
    public class ShipWeaponController : MonoBehaviour {
        [SerializeField]
        private float WeaponsAngle;
        [SerializeField]
        private List<ShipWeapon> _Weapons;
        // public float ImpulseForce;
        
        // private ShipMovementController _ShipMovementController;
        
        public Ship Ship { get; private set; }
        
        private void Awake() {
            Ship = GetComponent<Ship>();
            // _ShipMovementController = GetComponent<ShipMovementController>();
        }

        public void Setup() {
            Ship.ShipModel.WeaponsContainer.GetComponentsInChildren(_Weapons);
            var weaponsRot = Quaternion.Euler(0, 0, WeaponsAngle);
            _Weapons.ForEach(_ => _.transform.localRotation = weaponsRot);
        }

        public void Fire() {
            _Weapons.ForEach(_ => _.Fire());
            // _ShipMovementController.ApllyImpulse(-transform.right * ImpulseForce);
        }
    }
}
