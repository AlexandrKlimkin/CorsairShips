using UnityEngine;

namespace Game.SeaGameplay {
    public class ShipPlayerController : MonoBehaviour {

        private ShipMovementController _MovementController;
        private ShipWeaponController _WeaponController;
        
        private void Awake() {
            _MovementController = GetComponent<ShipMovementController>();
            _WeaponController = GetComponent<ShipWeaponController>();
        }

        private void Update() {
            var vertAxis = Input.GetAxis("Vertical");
            var horAxis = Input.GetAxis("Horizontal");
            _MovementController.HorAxis = horAxis;
            _MovementController.VertAxis = vertAxis;
            if (Input.GetKeyDown(KeyCode.Space)) {
                _WeaponController.Fire();
            }
        }
    }
}