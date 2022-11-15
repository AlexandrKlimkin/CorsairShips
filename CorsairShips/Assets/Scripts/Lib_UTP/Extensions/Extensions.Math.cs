using UnityEngine;

namespace UTPLib.Core.Utils {
    public static partial class Extensions {
        public static Vector2 ToVector2XY(this Vector3 vector) {
            return new Vector2(vector.x, vector.y);
        }

        public static Vector2 ToVector2XZ(this Vector3 vector) {
            return new Vector2(vector.x, vector.z);
        }

        public static Vector3 ToVector3XZ(this Vector2 vector) {
            return new Vector3(vector.x, 0, vector.y);
        }
    }
}