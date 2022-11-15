using UnityEngine;

namespace PestelLib.Utils
{
    public class SimpleRotateTransform : MonoBehaviour
    {
        private Transform _transform;

        public Vector3 Speed = new Vector3(0, 0, 0);

        private void Start()
        {
            _transform = transform;
        }

        private void Update()
        {
            Quaternion offset = Quaternion.Euler(Speed*Time.deltaTime);
            _transform.localRotation = _transform.localRotation * offset;
        }
    }
}