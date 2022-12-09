using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Markers.Pointers {
    public class PointerMarkerWidget : MarkerWidget<PointerMarkerData> {
        
        [SerializeField]
        private RectTransform _MarkerRoot;
        [SerializeField]
        private RectTransform _PreviewContainer;
        [SerializeField]
        private Image _Icon;
        [SerializeField]
        private TextMeshProUGUI _DistanceText;
        [SerializeField]
        private List<Image> _BGImages;
        
        private Vector2 _ScreenPos;
        private Bounds _CameraBounds;
        private bool _InCameraRect;

        private PointerMarkerData _Data;
        
        protected override Vector2 TransformPosition(Vector3 position) {
            //ToDo: remove camera main, cache camera
            var viewportPoint = Camera.main.WorldToViewportPoint(position);

            _InCameraRect = viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 &&
                            viewportPoint.y < 1;

            if (_InCameraRect) {
                var pos = Vector3.Scale(new Vector3(_ParentRect.width, _ParentRect.height), viewportPoint - Vector3.one * 0.5f);
                transform.localRotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.down, pos));
                return pos;
            }

            if (viewportPoint.z < 0) {
                viewportPoint.x = 1 - viewportPoint.x;
                viewportPoint.y = 1 - viewportPoint.y;
            }

            var screenPos =  Vector3.Scale(new Vector3(_ParentRect.width, _ParentRect.height), viewportPoint);
            

            var screenCenter = new Vector3(_ParentRect.width, _ParentRect.height, 0) / 2f;
            screenPos -= screenCenter;

            float angle = Mathf.Atan2(screenPos.y, screenPos.x);
            angle -= 90 * Mathf.Deg2Rad;

            float cos = Mathf.Cos(angle);
            float sin = -Mathf.Sin(angle);

            float m = cos / sin;

            var screenBounds = screenCenter;
            if (_Data != null)
                screenBounds = new Vector3(screenBounds.x - _Data.BordersOffset, screenBounds.y - _Data.BordersOffset);

            if (cos > 0) {
                screenPos = new Vector3(screenBounds.y / m, screenBounds.y, 0);
            }
            else {
                screenPos = new Vector3(-screenBounds.y / m, -screenBounds.y, 0);
            }

            if (screenPos.x > screenBounds.x) {
                screenPos = new Vector3(screenBounds.x, screenBounds.x * m, 0);
            } else if (screenPos.x < -screenBounds.x) {
                screenPos = new Vector3(-screenBounds.x, -screenBounds.x * m, 0);
            }

            transform.localRotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.down, screenPos));
            
            return screenPos;
        }
        
        
        protected override void HandleData(PointerMarkerData data) {
            _Data = data;
            _MarkerRoot.gameObject.SetActive(!_InCameraRect);
            if (_InCameraRect)
                return;
            _PreviewContainer.gameObject.SetActive(data.ShowPreview);
            if (data.ShowPreview) {
                _Icon.sprite = data.IconSprite;
                _Icon.rectTransform.rotation = Quaternion.identity;
                _Icon.preserveAspect = true;
            }
            // _MarkerRoot.localPosition = new Vector3(0, data.BordersOffset, 0);
            _DistanceText.gameObject.SetActive(data.ShowDistance);
            if (data.ShowDistance) {
                _DistanceText.text = $"{Mathf.Round(data.Distance)}";
                _DistanceText.rectTransform.rotation = Quaternion.identity;
                _DistanceText.color = data.DistanceTextColor;
            }
            _BGImages?.ForEach(_ => _.color = data.BGColor);
        }
    }
}
