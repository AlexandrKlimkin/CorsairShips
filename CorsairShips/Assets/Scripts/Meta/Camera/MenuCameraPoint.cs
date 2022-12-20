using System;
using UnityEngine;

namespace Menu.Camera {
    [Serializable]
    public struct MenuCameraPoint {
        public float RotationAngle;
        public float ElevationAngle;
        public float Distance;
        public Vector3 Offset;

        public static MenuCameraPoint Lerp(MenuCameraPoint from, MenuCameraPoint to, float fraction) {
            return new MenuCameraPoint() {
                RotationAngle = Mathf.LerpAngle(from.RotationAngle, to.RotationAngle, fraction),
                ElevationAngle = Mathf.Lerp(from.ElevationAngle, to.ElevationAngle, fraction),
                Distance = Mathf.Lerp(from.Distance, to.Distance, fraction),
                Offset = Vector3.Lerp(from.Offset, to.Offset, fraction)
            };
        }
    }
}