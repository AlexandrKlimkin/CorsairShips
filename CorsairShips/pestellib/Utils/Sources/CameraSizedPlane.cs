using UnityEngine;

namespace PestelLib.Utils
{
    public class CameraSizedPlane : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        private void Start()
        {
            if (_camera == null)
            {
                while (_camera == null)
                {
                    _camera = transform.parent.GetComponent<Camera>();
                }
            }

            if (_camera == null)
            {
                Debug.LogError("CameraSizedPlane needs reference to Camera or be child of Camera");
                enabled = false;
                return;
            }
        }

        public void SetCamera(Camera cam)
        {
            _camera = cam;
        }

        private void Update()
        {
            float distToCamera = (transform.position - _camera.transform.position).magnitude;
            float height = 2.0f * Mathf.Tan(0.5f * _camera.fieldOfView * Mathf.Deg2Rad) * distToCamera;
            float width = height * _camera.aspect;

            transform.localScale = new Vector3(width, height, 1f);
        }
    }
}