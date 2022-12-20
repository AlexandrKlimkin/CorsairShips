using System;
using System.Collections.Generic;
using UnityDI;
using UnityEngine;
using UnityEngine.EventSystems;
using UTPLib.Core.Utils;

namespace Menu.Camera {
    public class MenuCameraInputPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
        [Dependency]
        private readonly MenuCameraController _CameraController;
        
        private bool _WasDragged;
        private readonly List<int> _PointerIDs = new();
        private float _LastScaleDistance;
        private float _TargetScaleDelta;
        private Vector2 _TargetTouchDelta;
        private Vector2 _CurrentTouchDelta;
#if UNITY_EDITOR || UNITY_STANDALONE
        private Vector2 _LastMousePos;
#endif

        private void Start() {
            ContainerHolder.Container.BuildUp(this);
        }

        private void Update() {
            ProcessTouches();
            _CameraController.SetTouchDelta(_TargetTouchDelta);
            // _CameraController.SetZoomDelta(_TargetScaleDelta);
        }

        private void ProcessTouches() {

            if (_PointerIDs.Count == 1) {
                // Move on 1 touch
#if UNITY_EDITOR || UNITY_STANDALONE
                _TargetTouchDelta =
                    (Input.mousePosition.ToVector2XY() - _LastMousePos) / 1.5f; // Simulate touch with mouse
#else
            var touchID = _PointerIDs[0];
            for (int i = 0; i < Input.touches.Length; i++) {
                if (Input.touches[i].fingerId == touchID) {
                    _TargetTouchDelta = Input.touches[i].deltaPosition;
                    _TargetTouchDelta.y *= -1;
                }
            }
#endif
            }
            else {
                _TargetTouchDelta = Vector2.zero;
            }

            if (_PointerIDs.Count == 2) {
                // Zoom on 2 touches
                Touch touch1 = new Touch();
                Touch touch2 = new Touch();
                var touchID1 = _PointerIDs[0];
                var touchID2 = _PointerIDs[1];
                for (int i = 0; i < Input.touches.Length; i++) {
                    if (Input.touches[i].fingerId == touchID1) {
                        touch1 = Input.touches[i];
                    }

                    if (Input.touches[i].fingerId == touchID2) {
                        touch2 = Input.touches[i];
                    }
                }

                var distance = (touch1.position - touch2.position).magnitude;

                if (_LastScaleDistance != 0) {
                    _TargetScaleDelta = _LastScaleDistance - distance;
                }
                else {
                    _TargetScaleDelta = 0;
                }

                _LastScaleDistance = distance;
            }
            else {
                _TargetScaleDelta = 0;
                _LastScaleDistance = 0;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            _LastMousePos = Input.mousePosition;
#endif
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (_PointerIDs.Count < 2 && !_PointerIDs.Contains(eventData.pointerId)) {
                _PointerIDs.Add(eventData.pointerId);
            }
        }

        public void OnPointerUp(PointerEventData eventData) {
            _PointerIDs.Remove(eventData.pointerId);
        }

        private void OnApplicationPause(bool pause) {
            if (pause)
                _PointerIDs.Clear();
        }
    }
}