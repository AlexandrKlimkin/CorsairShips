using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NorskaLib.Utilities
{
    public struct Vector3Utils
    {
        public static Vector3 ComponentMult (Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vector3 Uniform(float value)
        {
            return new Vector3(value, value, value);
        }
    }
}
