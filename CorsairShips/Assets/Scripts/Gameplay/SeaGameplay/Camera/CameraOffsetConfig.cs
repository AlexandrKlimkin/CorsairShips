using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.SeaGameplay.Camera {
    [CreateAssetMenu(fileName = "CameraOffsetConfig", menuName = "Configs/CameraOffsetConfig")]
    public class CameraOffsetConfig : ScriptableObject {
        public Vector3 UsualOffset;
        public List<ShipCameraSettings> CameraExtensions;
    }

    [Serializable]
    public class ShipCameraSettings {
        public Vector3 Offest;
        public string ShipId;
    }
}
