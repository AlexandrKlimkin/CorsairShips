using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Rails {
    public class Rail : MonoBehaviour {

        public Vector3 MidPoint { get; private set; }
        public Vector3 EndPoint => _RailEnd.position;
        public bool Initialized { get; private set; }

        private Transform _RailStart;
        private Transform _RailEnd;
        private float _Length;

        private void Awake() {
            _RailStart = this.transform.GetChild(0);
            _RailEnd = this.transform.GetChild(1);
            _Length = Vector3.Distance(_RailStart.position, _RailEnd.position);
        }

        private void Start() {
            UpdateMidPoint();
        }

        private void FixedUpdate() {
            UpdateMidPoint();
        }

        private void UpdateMidPoint() {
            Initialized = true;
            MidPoint = (_RailStart.position + _RailEnd.position) / 2;
        }

        /// <summary>
        /// Creates a circe point projection on the segment
        /// </summary>
        /// <param name="direction"> Direction of projection, must be normalized</param>
        /// <returns></returns>
        public Vector3 Project(Vector3 direction) {
            var forward = this.transform.forward;
            return MidPoint - forward * Vector3.Dot(direction, forward) * _Length / 2;
        }

        public Vector3 Raycast(Vector3 position, Vector3 direction) {
            return GetClosestPointOnSegment(MidPoint, this.transform.forward, position, direction, _Length);
        }

        public Vector3 GetLerpPoint(float t) {
            return Vector3.Lerp(_RailStart.position, _RailEnd.position, t);
        }

        // All directions should be normalized
        private Vector3 GetClosestPointOnSegment(Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2,
            Vector3 lineVec2,
            float length) {

            float b = Vector3.Dot(lineVec1, lineVec2);

            float d = 1 - b * b;

            var canProfil = true;
            var targetDir = MidPoint - linePoint2;
            var angle = Vector3.SignedAngle(lineVec1, targetDir, Vector3.up);
            var absAngle = Mathf.Abs(angle);
            if (absAngle < 10 || absAngle > 170)
                canProfil = false;

            //lines are not parallel
            if (d != 0.0f && canProfil) {

                Vector3 r = linePoint1 - linePoint2;
                float c = Vector3.Dot(lineVec1, r);
                float f = Vector3.Dot(lineVec2, r);

                float s = Mathf.Clamp((b * f - c) / d, -length / 2, +length / 2);

                return linePoint1 + lineVec1 * s;
            }
            else {
                return MidPoint;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Awake();
            UnityEditor.Handles.DrawLine(_RailStart.position, _RailEnd.position);
            if (Camera.current != null) {
                var viewDir = Camera.current.transform.forward;
                var viewPos = Camera.current.transform.position;
                UnityEditor.Handles.DrawWireDisc(this.Raycast(viewPos, viewDir), viewDir, 0.1f);
            }
        }
#endif
    }
}
