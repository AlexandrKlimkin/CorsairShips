using System;
using UnityEngine;

namespace UTPLib.Core.Utils {
    public static partial class Extensions {
        public static void ClearChildren(this Transform transform, Action<Transform> deactivator) {
            for (int i = 0; i < transform.childCount; i++) {
                deactivator(transform.GetChild(i));
            }
        }
    
        public static void ClearChildren(this Transform transform, bool immediate = false) {
            if (immediate) {
                while(transform.childCount > 0) {
                    GameObject.DestroyImmediate(transform.GetChild(0).gameObject);
                }
            }
            else {
                transform.ClearChildren(_ => GameObject.Destroy(_.gameObject));
            }
        }
    }
}