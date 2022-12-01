using System.Collections;
using System.Collections.Generic;
using PestelLib.UI;
using UnityDI;
using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace Game.SeaGameplay.UI {
    public class MoveJoystickPositioner : OnScreenControl {

        public Vector2 joystickZoneSizePct = new Vector2(0.4f, 1);

        [SerializeField]
        private RectTransform joystickCircleBig;

        [SerializeField]
        private RectTransform joystickCircleSmall;

        [Space]
        [SerializeField]
        private PointerAnimations joystickPointerAnimation;

        [SerializeField]
        private bool stickToFirstTouch;

        [SerializeField]
        private OnScreenStick stick;

        [Dependency]
        private Gui _gui;

        private Vector2 baseJoystickPosition;
        private Vector2 joysticStartingPosition;

        private RectTransform rectTransform;
        private CanvasScaler canvasScaler;
        private int joysticFingerId = -1;

        [InputControl(layout = "Vector2")]
        [SerializeField]
        private string m_ControlPath;

        protected override string controlPathInternal {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }

        private void Awake() {
            ContainerHolder.Container.BuildUp(this);
            rectTransform = GetComponent<RectTransform>();
            canvasScaler = _gui.Canvas.GetComponent<CanvasScaler>();

            joysticStartingPosition = rectTransform.anchoredPosition;
            baseJoystickPosition = rectTransform.anchoredPosition;
        }

        private void Update() {
            UpdateJoystickPosition();
        }

        void UpdateJoystickPosition() {
#if UNITY_EDITOR
            Vector2 inputPosition = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
                UpdateOnTap(inputPosition, 0);
            else if (Input.GetMouseButton(0))
                UpdateOnProgress(inputPosition, 0);
            else if (Input.GetMouseButtonUp(0))
                UpdateEnded();

#endif

            for (int i = 0; i < Input.touchCount; i++) {
                var touch = Input.GetTouch(i);
                switch (touch.phase) {
                    case TouchPhase.Began:
                        UpdateOnTap(touch.position, touch.fingerId);
                        break;
                    case TouchPhase.Moved:
                        UpdateOnProgress(touch.position, touch.fingerId);
                        break;
                    case TouchPhase.Ended:
                        if (touch.fingerId == joysticFingerId) UpdateEnded();
                        break;
                }
            }

            // if (joysticFingerId == -1)
            // {
            //     joystickCircleBig.anchoredPosition = Vector2.MoveTowards(joystickCircleBig.anchoredPosition, joystickCircleSmall.anchoredPosition, 8 * joystickSize * Time.deltaTime);
            // }

        }

        private void UpdateOnTap(Vector2 tapPosition, int id) {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) {
                return;
            }

            if (tapPosition.x / Screen.width < joystickZoneSizePct.x &&
                tapPosition.y / Screen.height < joystickZoneSizePct.y) {
                joysticFingerId = id;

                var aspect = Screen.width * 1f / Screen.height;
                Vector2 touchPositionInCanvas = tapPosition;
                touchPositionInCanvas.x *= canvasScaler.referenceResolution.x / Screen.width;
                var canvasHeight = canvasScaler.referenceResolution.x / aspect;
                touchPositionInCanvas.y *= canvasHeight / Screen.height;

                rectTransform.anchoredPosition = touchPositionInCanvas;
                joystickCircleBig.anchoredPosition = Vector2.zero;
                joystickCircleSmall.anchoredPosition = Vector2.zero;

                joysticStartingPosition = touchPositionInCanvas;
            }

            joystickPointerAnimation.AnimateDown();
        }

        private void UpdateOnProgress(Vector2 tapPosition, int fingerId) {
            if (fingerId == joysticFingerId) {
                var aspect = Screen.width * 1f / Screen.height;
                Vector2 touchPositionInCanvas = tapPosition;
                touchPositionInCanvas.x *= canvasScaler.referenceResolution.x / Screen.width;
                var canvasHeight = canvasScaler.referenceResolution.x / aspect;
                touchPositionInCanvas.y *= canvasHeight / Screen.height;

                var deltaClamped = Vector2.ClampMagnitude(
                    touchPositionInCanvas - (stickToFirstTouch ? joysticStartingPosition : baseJoystickPosition),
                    stick.movementRange);
                joystickCircleSmall.anchoredPosition = deltaClamped;

                var input = deltaClamped / stick.movementRange;

                SendValueToControl(input);
            }
        }

        private void UpdateEnded() {
            joysticFingerId = -1;
            rectTransform.anchoredPosition = baseJoystickPosition;
            joystickCircleBig.anchoredPosition = Vector2.zero;
            joystickCircleSmall.anchoredPosition = Vector2.zero;

            SendValueToControl(Vector2.zero);

            joystickPointerAnimation.AnimateUp();
        }
    }
}
