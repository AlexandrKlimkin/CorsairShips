using UnityDI;
using UnityEngine;

namespace PestelLib.UI
{
    public class AlignToWorldObject : InitableMonoBehaviour
    {
        public CanvasGroup CanvasGroup;
        public Transform Target; //TODO Make this private. Better to setup it in method or property to change position immediately after set target
        public RectTransform ParentRectTransform;
        [SerializeField] private GameObject _arrow;
        public Vector3 WorldOffset;
        public Vector2 ScreenOffset;
        public bool IsAlwaysOnScreen;
        public bool VisibleOnlyOnBorder;
        public bool LookAtOffscreenTarget;
        public Camera OverrideCamera;
        [Dependency] private CameraStack _cameraStack;

        public bool InverseOnBackSide;

        Rect rect;

        public float edgeBuffer;

        protected virtual Camera Camera
        {
            get {
                if (OverrideCamera != null)
                    return OverrideCamera;

                return _cameraStack.TopCamera;
            }
        }

        protected override void SafeStart()
        {
            rect = transform.root.GetComponent<RectTransform>().rect;
        }

        public void SetTarget(Transform target)
        {
            Target = target;
            UpdatePosition();
        }

        private void LateUpdate()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (Target == null || Camera == null)
            {
                return;
            }

            Vector3 positionInViewport = WorldToScreenPointProjected(Camera, Target.position + WorldOffset);

            UpdateCanvasGroupAlpha(positionInViewport, out var canvasVisible);

            if (!canvasVisible) return;

            var screenPos = new Vector3(
                positionInViewport.x * rect.width + ScreenOffset.x, //*_root.localScale.x,
                positionInViewport.y * rect.height + ScreenOffset.y, //*_root.localScale.y,
                0
            );

            var onScreen = 0 <= positionInViewport.x && positionInViewport.x <= 1 && 
                           0 <= positionInViewport.y && positionInViewport.y <= 1;

            if (onScreen || !IsAlwaysOnScreen)
            {
                transform.localPosition = screenPos;
                transform.localRotation = Quaternion.identity;

                if (_arrow != null)
                {
                    _arrow.SetActive(false);
                }
            }
            else
            {
                transform.localPosition = ScreenPointEdgeClamp(rect.width, rect.height, screenPos, edgeBuffer, out var a);

                if (LookAtOffscreenTarget)
                {
                    transform.localRotation = Quaternion.Euler(0, 0, -a);
                }

                if (_arrow != null)
                {
                    _arrow.SetActive(true);
                    _arrow.transform.localRotation = Quaternion.Euler(0, 0, -a);
                }
            }
        }

        protected bool IsOnScreen(Vector3 positionInViewport)
        {
            return 0 <= positionInViewport.x && positionInViewport.x <= 1 &&
                   0 <= positionInViewport.y && positionInViewport.y <= 1;
        }

        protected virtual void UpdateCanvasGroupAlpha(Vector3 positionInViewport, out bool canvasVisible)
        {
            if (CanvasGroup != null)
            {
                CanvasGroup.alpha = Target.gameObject.activeInHierarchy ? 1 : 0;

                if (VisibleOnlyOnBorder)
                {
                    CanvasGroup.alpha = IsOnScreen(positionInViewport) ? 0 : 1;
                }
            }

            canvasVisible = (CanvasGroup == null) || !Mathf.Approximately(CanvasGroup.alpha, 0);
        }

        //https://forum.unity.com/threads/camera-worldtoscreenpoint-bug.85311/?_ga=2.239022848.1674515220.1598447111-1245495264.1497275759#post-2844282
        public static Vector2 WorldToScreenPointProjected(Camera camera, Vector3 worldPos)
        {
            Vector3 camNormal = camera.transform.forward;
            Vector3 vectorFromCam = worldPos - camera.transform.position;
            float camNormDot = Vector3.Dot(camNormal, vectorFromCam);
            if (camNormDot <= 0)
            {
                // we are behind the camera forward facing plane, project the position in front of the plane
                Vector3 proj = (camNormal * camNormDot * 1.01f);
                worldPos = camera.transform.position + (vectorFromCam - proj);
            }

            return camera.WorldToViewportPoint(worldPos);
            //return RectTransformUtility.WorldToScreenPoint(camera, worldPos);
        }

        public static Vector3 ScreenPointEdgeClamp(float width, float height, Vector2 screenPos, float edgeBuffer, out float angleDeg)
        {
            // Take the direction of the screen point from the screen center to push it out to the edge of the screen
            // Use the shortest distance from projecting it along the height and width
            Vector2 screenCenter = new Vector2(width / 2.0f, height / 2.0f);
            Vector2 screenDir = (screenPos - screenCenter).normalized;
            float angleRad = Mathf.Atan2(screenDir.x, screenDir.y);
            float distHeight = Mathf.Abs((screenCenter.y - edgeBuffer) / Mathf.Cos(angleRad));
            float distWidth = Mathf.Abs((screenCenter.x - edgeBuffer) / Mathf.Cos(angleRad + (Mathf.PI * 0.5f)));
            float dist = Mathf.Min(distHeight, distWidth);
            angleDeg = angleRad * Mathf.Rad2Deg;
            return screenCenter + (screenDir * dist);
        }
    }
}