using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.SeaGameplay {
    public class ShipPlayerController : MonoBehaviour {

        private ShipMovementController _MovementController;
        private ShipWeaponController _WeaponController;
        
        private InputControls _InputControls;
        
        private InputAction _MoveAction;
        private InputAction _FireAction;
        
        private void Awake() {
            _MovementController = GetComponent<ShipMovementController>();
            _WeaponController = GetComponent<ShipWeaponController>();

            _InputControls = new InputControls();
            _MoveAction = _InputControls.ShipControlls.Move;
            _FireAction = _InputControls.ShipControlls.Fire;
            
            _FireAction.started += FireActionOnStarted;
        }

        private void FireActionOnStarted(InputAction.CallbackContext obj) {
            _WeaponController.Fire();
        }

        private void OnEnable() {
            _MoveAction.Enable();
            _FireAction.Enable();
        }

        private void OnDisable() {
            _MoveAction.Disable();
            _FireAction.Disable();
        }

        private void Update() {

            var moveVector = _MoveAction.ReadValue<Vector2>();
            
            var vertAxis = moveVector.y;
            var horAxis = moveVector.x;
            _MovementController.Gaz = moveVector.magnitude;
            _MovementController.Direction = moveVector;
            
        }
    }
}