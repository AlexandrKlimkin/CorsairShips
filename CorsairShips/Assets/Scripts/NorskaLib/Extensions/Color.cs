using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NorskaLib.Extensions
{
    public static class ColorExtensions
    {
        public static Color WithA(this Color instance, float a)
        {
            return new Color (instance.r, instance.g, instance.b, a);
        }
    }
}