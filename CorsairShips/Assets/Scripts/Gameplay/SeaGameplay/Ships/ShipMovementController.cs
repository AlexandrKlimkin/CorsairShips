using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.SeaGameplay {
    public class ShipMovementController : MonoBehaviour {

        public float MaxSpeed;
        public float Acceleration;
        public float Deceleration;
        public float RotateToForwardFactor;
        public float MaxRotateSpeed;
        public float RotateAcceleration;
        public float RotateDeceleration;
        
        public float Speed { get; private set; }
        public float RotateSpeed { get; private set; }
        
        public Rigidbody Rigidbody { get; private set; }

        public float HorAxis { get; set; }
        public float VertAxis { get; set; }
        
        private void Awake() {
            Rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate() {
            // var vertAxis = Input.GetAxis("Vertical");
            // var horAxis = Input.GetAxis("Horizontal");
            //
            ProcessRotate(HorAxis);
            ProcessMove(VertAxis);
        }

        public void Setup() {
            
        }
        
        private void ProcessMove(float vertAxis) {
            var currentVelocityDir = Rigidbody.velocity.sqrMagnitude > 0.01 ? Rigidbody.velocity.normalized : transform.forward;
            var speedDelta = 0f;
            
            if (Mathf.Abs(vertAxis) > 0.1) {
                if (Speed < MaxSpeed) {
                    speedDelta = Acceleration * Time.fixedDeltaTime * vertAxis;
                }
            }
            else {
                if (Speed > 0.01f) {
                    speedDelta = -Deceleration * Time.fixedDeltaTime;
                }
            }
            Speed += speedDelta;
            Speed = Mathf.Clamp(Speed, -MaxSpeed, MaxSpeed);

            var targetVelocity = transform.forward * Speed;
            var velocityDelta = currentVelocityDir * speedDelta;

            Rigidbody.velocity += velocityDelta;
            Rigidbody.velocity = Vector3.Lerp(Rigidbody.velocity, targetVelocity, Time.fixedDeltaTime * RotateToForwardFactor);
 
        }

        private void ProcessRotate(float horAxis) {

            var absRotSpeed = Mathf.Abs(RotateSpeed);
            
            var rotateDelta = 0f;

            if (Mathf.Abs(horAxis) > 0.1) {
                if (absRotSpeed < MaxRotateSpeed) {
                    rotateDelta = RotateAcceleration * Time.fixedDeltaTime * horAxis;
                }
            }
            else {
                if (absRotSpeed > 1f) {
                    var sign = RotateSpeed > 0 ? -1f : 1f;
                    rotateDelta = sign * RotateDeceleration * Time.fixedDeltaTime;
                }
                else {
                    rotateDelta = -RotateSpeed;
                }
            }
            RotateSpeed += rotateDelta;
            RotateSpeed = Mathf.Clamp(RotateSpeed, -MaxRotateSpeed, MaxRotateSpeed);

            var yDeltaDeg = RotateSpeed * Time.fixedDeltaTime;
            if (Mathf.Abs(yDeltaDeg) > 0.1) {
                var newRot = Rigidbody.rotation * Quaternion.Euler(0, yDeltaDeg, 0);
                Rigidbody.MoveRotation(newRot);
            }
        }

        private void OnDrawGizmos() {
            if (Application.isPlaying) {
                var centerPos = transform.position + Vector3.up * 2f;
                Gizmos.color = Color.green;
                Gizmos.DrawLine(centerPos, centerPos + transform.forward * 5f);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(centerPos, centerPos + Rigidbody.velocity);
            }
        }

        public void ApllyImpulse(Vector3 impulse) {
            Rigidbody.AddForce(impulse, ForceMode.Impulse);
        }
    }
}
