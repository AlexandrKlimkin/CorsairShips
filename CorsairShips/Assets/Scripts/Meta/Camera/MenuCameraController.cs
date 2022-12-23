using System;
using System.Collections;
using System.Collections.Generic;
using Game.SeaGameplay;
using UnityDI;
using UnityEngine;
using UTPLib.Core.Utils;

namespace Menu.Camera {
    public class MenuCameraController : MonoBehaviour {
        
        private Transform _BaseArm;
        private Transform _OffsetArm;
        private Transform _ElevationArm;
        
        [SerializeField]
        private float _MinDistance;
        [SerializeField]
        private float _MaxDistance;
        // public float Offset;
        // public float Height;
        // public float PositionDamping;
        // public float TouchDeltaDamping;
        
        private ICameraTarget _CameraTarget;

        private Vector3 _PlaneForward;
        private Vector3 _TargetPos;
        private Quaternion _TargetRot;
        
        public MenuCameraPoint TargetPoint;
        private MenuCameraPoint _CurrentPoint;
        private MenuCameraPosition _TargetCameraPosition;
        private MenuCameraPosition _CurrentCameraPosition;
        
        private bool _ControlledByInput = true;
        private float _ZoomDelta;
        private float _CurrentMinVAngle = MinVAngle;
        private float _CurrentMaxVAngle = MaxVAngle;
        
        private float _CurrentPointDamping;
        private float _CurrentPositionDamping;
        
        public const float MinVAngle = -5;
        public const float MaxVAngle = 35;
        
        public const float InputHSens = 0.125f;
        public const float InputVSens = 0.075f;
        public const float InputDSens = 0.001f;
        
        public const float PositionDampingSpeed = 1f;
        public const float MinPositionDamping = 0f;
        public const float MaxPositionDamping = 20f;
        
        public const float HeightByDistance = 0.1f;
        public const float MinPointDamping = 0f;
        public const float MaxPointDamping = 10f;
        
        public const float PointDampingSpeed = 0.25f;
        
        public void SetTarget(ICameraTarget target) {
            _CameraTarget = target;
        }

        private void Awake() {
            _BaseArm = this.transform;
            _ElevationArm = _BaseArm.GetChild(0);
            _OffsetArm = _ElevationArm.GetChild(0);
            _PlaneForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
            ContainerHolder.Container.RegisterInstance(this);
        }

        private void Start() {
            
        }

        private void Update() {
            _CameraTarget ??= FindObjectOfType<ShipModelController>(false);
            if(_CameraTarget == null)
                return;
            ProcessInput();
            UpdatePointDamping();
            UpdatePositionDamping();
            
            // UpdatePosition();
            // // UpdateRotation();
            // UpdatePlaneForward();
            // UpdateTouchDelta();
            
            ProcessCameraPoint(TargetPoint, _CurrentPointDamping);
            _TargetCameraPosition = GetTargetCameraPos(_CurrentPoint);
            ProcessCameraPosition(_TargetCameraPosition, _CurrentPositionDamping);
            // _TargetCameraPosition = GetTargetCameraPos()
        }

        // private void UpdatePosition() {
        //     _TargetPos = _CameraTarget.ViewPoint - (_PlaneForward * Offset) + (Vector3.up * Height);
        //     transform.position = Vector3.Lerp(transform.position, _TargetPos, Time.deltaTime * PositionDamping);
        // }

        // private void UpdateRotation() {
        //     _TargetRot = Quaternion.LookRotation()
        //     
        //     _TargetRot = Quaternion.Euler(Angle, transform.rotation.eulerAngles.y, 0);
        //     transform.rotation = Quaternion.Lerp(transform.rotation, _TargetRot, Time.deltaTime * RotationDamping);
        // }
        
        // private void UpdatePlaneForward() {
        //     _PlaneForward = Quaternion.Euler(0, _TouchDelta.x, 0) * _PlaneForward;
        // }
        //
        // private void UpdateTouchDelta() {
        //     _TouchDelta = Vector2.Lerp(_TouchDelta, Vector2.zero, Time.deltaTime * TouchDeltaDamping);
        // }
        
        private Vector2 _TouchDelta;
        public void SetTouchDelta(Vector2 delta) {
            _TouchDelta = delta;
        }
        
        private void ProcessCameraPoint(MenuCameraPoint targetCameraPoint, float damping) {
            _CurrentPoint = MenuCameraPoint.Lerp(_CurrentPoint, targetCameraPoint, Time.deltaTime * 5f);
        }
        
        private void UpdatePointDamping() {
            _CurrentPointDamping = Mathf.Lerp(_CurrentPointDamping, 1, PointDampingSpeed * Time.deltaTime);
        }
        
        private void UpdatePositionDamping() {
            _CurrentPositionDamping = Mathf.Lerp(_CurrentPositionDamping, 1, PositionDampingSpeed * Time.deltaTime);
        }
        
        private void ProcessInput() {
            if (_ControlledByInput) {
                TargetPoint.RotationAngle += _TouchDelta.x * InputHSens;
                TargetPoint.ElevationAngle += -_TouchDelta.y * InputVSens;
                TargetPoint.Distance += _ZoomDelta * InputDSens;

                TargetPoint.ElevationAngle = Mathf.Clamp(TargetPoint.ElevationAngle, _CurrentMinVAngle, _CurrentMaxVAngle);
                TargetPoint.Distance = Mathf.Clamp(TargetPoint.Distance, _MinDistance, _MaxDistance);
            }
        }
        
        private MenuCameraPosition GetTargetCameraPos(MenuCameraPoint cameraPoint) {
            
            var baseForward = Vector3.forward;
            var planeForward = Quaternion.Euler(0, cameraPoint.RotationAngle, 0) * baseForward;
            var railPosition = _CameraTarget.GetProjectPoint(planeForward);
            // var distanceOffset = Mathf.Lerp(MinDistance, MaxDistance, cameraPoint.Distance);
            var distanceOffset = cameraPoint.Distance;
            var cameraOffset = Vector3.back * distanceOffset + Vector3.up * distanceOffset * HeightByDistance + cameraPoint.Offset;

            return new MenuCameraPosition() {
                BasePosition = railPosition,
                BaseRotation = Quaternion.LookRotation(planeForward),
                OffsetArmPosition = cameraOffset,
                ElevationArmRotation = Quaternion.Euler(cameraPoint.ElevationAngle, 0, 0)
            };
        }
        
        private void ProcessCameraPosition(MenuCameraPosition targetCameraPos, float damping) {
            var resultingPos = MenuCameraPosition.Lerp(_CurrentCameraPosition, targetCameraPos, Time.deltaTime * Mathf.Lerp(MinPositionDamping, MaxPositionDamping, damping));
            ApplyCameraPosition(resultingPos);
        }

        private void ApplyCameraPosition(MenuCameraPosition cameraPos) {
            _CurrentCameraPosition = cameraPos;
            _BaseArm.localPosition = cameraPos.BasePosition;
            _BaseArm.localRotation = cameraPos.BaseRotation;
            _OffsetArm.localPosition = cameraPos.OffsetArmPosition;
            _ElevationArm.localRotation = cameraPos.ElevationArmRotation;
        }
        
        private struct MenuCameraPosition {
            public Vector3 BasePosition;
            public Quaternion BaseRotation;
            public Vector3 OffsetArmPosition;
            public Quaternion ElevationArmRotation;

            public static MenuCameraPosition Lerp(MenuCameraPosition from, MenuCameraPosition to, float fraction) {
                return new MenuCameraPosition() {
                    BasePosition = Vector3.Lerp(from.BasePosition, to.BasePosition, fraction),
                    BaseRotation = Quaternion.Slerp(from.BaseRotation, to.BaseRotation, fraction),
                    OffsetArmPosition = Vector3.Lerp(from.OffsetArmPosition, to.OffsetArmPosition, fraction),
                    ElevationArmRotation = Quaternion.Slerp(from.ElevationArmRotation, to.ElevationArmRotation, fraction)
                };
            }
        }
    }
}
