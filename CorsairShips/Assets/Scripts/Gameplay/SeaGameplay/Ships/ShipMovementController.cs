using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UTPLib.Core.Utils;

namespace Game.SeaGameplay {
    public class ShipMovementController : MonoBehaviour {

        public float MaxSpeed;
        public float Acceleration;
        public float Deceleration;
        public float RotateToForwardFactor;
        [Space]
        public float MaxRotateYSpeed;
        public float RotateYAcceleration;
        public float RotateYDeceleration;
        public float RotateYSlowAngle;
        // public float MaxZRotation;
        
        // [Space]
        // public float MaxZAngle;
        // public float MaxRotateZSpeed;
        // public float RotateZAcceleration;
        // public float RotateZDeceleration;
        // public float RotateZSlowAngle;
        
        public float Speed { get; private set; }
        public float RotateSpeed { get; private set; }
        
        public Ship Ship { get; private set; }
        public Rigidbody Rigidbody { get; private set; }

        public Vector2 Direction { get; set; }
        public float Gaz { get; set; }
        
        // public float HorAxis { get; set; }
        // public float VertAxis { get; set; }
        
        private void Awake() {
            Ship = GetComponent<Ship>();
            Rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate() {
            // var vertAxis = Input.GetAxis("Vertical");
            // var horAxis = Input.GetAxis("Horizontal");
            //
            ProcessRotate();
            ProcessMove();
        }

        public void Setup() {
            
        }
        
        private void ProcessMove() {
            var currentVelocityDir = Rigidbody.velocity.sqrMagnitude > 0.01 ? Rigidbody.velocity.normalized : transform.forward;
            var speedDelta = 0f;

            var targetSpeed = Mathf.Lerp(0, MaxSpeed, Gaz);
            
            if (Speed < targetSpeed) {
                speedDelta = Acceleration * Time.fixedDeltaTime;
            }
            else {
                if (Speed > 0.01f) {
                    speedDelta = -Deceleration * Time.fixedDeltaTime;
                }
            }
            Speed += speedDelta;
            if(targetSpeed != 0)
                Speed = Mathf.Clamp(Speed, 0, MaxSpeed);

            var targetVelocity = transform.forward * Speed;
            var velocityDelta = currentVelocityDir * speedDelta;

            Rigidbody.velocity += velocityDelta;
            Rigidbody.velocity = Vector3.Lerp(Rigidbody.velocity, targetVelocity, Time.fixedDeltaTime * RotateToForwardFactor);
 
        }

        private void ProcessRotate() {

            var currentDir = transform.forward.ToVector2XZ();
            
            var absRotSpeed = Mathf.Abs(RotateSpeed);
            
            var rotateDelta = 0f;

            var angle = Vector2.SignedAngle(currentDir, Direction);

            //Debug.LogError(angle);

            var t = Mathf.InverseLerp(0, RotateYSlowAngle * Mathf.Sign(angle), angle);
            var maxRotateSpeed = Mathf.Lerp(0, MaxRotateYSpeed, t);
            
            if (Mathf.Abs(angle) > 1) {
                if (absRotSpeed < maxRotateSpeed) {
                    rotateDelta = RotateYAcceleration * Time.fixedDeltaTime * -Mathf.Sign(angle);
                }
            }
            else {
                if (absRotSpeed > 1f) {
                    var sign = RotateSpeed > 0 ? -1f : 1f;
                    rotateDelta = sign * RotateYDeceleration * Time.fixedDeltaTime;
                }
                else {
                    rotateDelta = -RotateSpeed;
                }
            }
            
            RotateSpeed += rotateDelta;
            RotateSpeed = Mathf.Clamp(RotateSpeed, -maxRotateSpeed, maxRotateSpeed);
                
            var yDeltaDeg = RotateSpeed * Time.fixedDeltaTime;

            var normRotYSpeed = Mathf.InverseLerp(MaxRotateYSpeed, -MaxRotateYSpeed, RotateSpeed);
            // var zRot = Mathf.Lerp(-MaxZRotation, MaxZRotation, normRotYSpeed);
            
            var currentRotEuler = Rigidbody.rotation.eulerAngles;
            
            if (Mathf.Abs(yDeltaDeg) > 0.1) {
                var newRot = Rigidbody.rotation * Quaternion.Euler(-currentRotEuler.x, yDeltaDeg, -currentRotEuler.z);
                Rigidbody.MoveRotation(newRot);
            }
            // var rbEulerRot = Rigidbody.rotation.eulerAngles;
            // Rigidbody.rotation = Quaternion.Euler(rbEulerRot.x, rbEulerRot.y, zRot);
            
            // if (Mathf.Abs(angle) > 0.1) {
            //     if (absRotSpeed < MaxRotateSpeed) {
            //         rotateDelta = RotateAcceleration * Time.fixedDeltaTime * -Mathf.Sign(angle);
            //     }
            // }
            // else {
            //     if (absRotSpeed > 1f) {
            //         var sign = RotateSpeed > 0 ? -1f : 1f;
            //         rotateDelta = sign * RotateDeceleration * Time.fixedDeltaTime;
            //     }
            //     else {
            //         rotateDelta = -RotateSpeed;
            //     }
            // }
            // RotateSpeed += rotateDelta;
            // RotateSpeed = Mathf.Clamp(RotateSpeed, -MaxRotateSpeed, MaxRotateSpeed);
            //
            // var yDeltaDeg = RotateSpeed * Time.fixedDeltaTime;
            // if (Mathf.Abs(yDeltaDeg) > 0.1) {
            //     var newRot = Rigidbody.rotation * Quaternion.Euler(0, yDeltaDeg, 0);
            //     Rigidbody.MoveRotation(newRot);
            // }
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

        public void ApplyImpulse(Vector3 impulse) {
            Rigidbody.AddForce(impulse, ForceMode.Impulse);
        }
    }
}
