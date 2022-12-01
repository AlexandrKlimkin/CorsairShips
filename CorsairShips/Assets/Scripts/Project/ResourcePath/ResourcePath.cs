using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class ResourcePath {
    public static class Ships {
        public const string ShipBasePrefabPath = "Prefabs/Ships/ShipBase";
        private const string ShipModelFolder = "Prefabs/Ships/";
        private const string ShipAIFolder = "Prefabs/AI/";
        
        public static string GetModelPath(string modelId) {
            return $"{ShipModelFolder}{modelId}/{modelId}";
        }

        public static string GetAIPath(string aiName) {
            return $"{ShipAIFolder}{aiName}";
        }
    }
    
    public static class Camera {
        public const string GameCameraPath = "Prefabs/Cameras/GameCamera";
    }
}
