using UnityEngine;

namespace CameraTools.Sources
{
    public class CameraSettings : MonoBehaviour
    {
        [SerializeField] private bool _isStaticCamera;

        private Camera _cam;
        private float _fov;
        private Quaternion _rotation;

        public bool IsStaticCamera
        {
            get { return _isStaticCamera; }
        }

        private void Start()
        {
            _cam = GetComponent<Camera>();
            _fov = _cam.fieldOfView;
            _rotation = _cam.transform.localRotation;
        }

        public void RestoreSettings()
        {
            _cam.fieldOfView = _fov;
            _cam.transform.localRotation = _rotation;
        }
    }
}
