using UnityEngine;

namespace PestelLib.Utils
{
    public class LinkTransform : MonoBehaviour
    {
        private Transform _target;

        public Transform Target
        {
            get { return _target; }
            set
            {
                _target = value;
                UpdatePosition();
            }
        }
        public Vector3 Offset = Vector3.zero;
        public bool IsInheritRotation;

        private void LateUpdate()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (transform == null || Target == null) return;

            transform.position = Target.position + Offset;
            if (IsInheritRotation)
                transform.rotation = Target.transform.rotation;
        }
    }
}
