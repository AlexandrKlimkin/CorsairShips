using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NorskaLib.Utilities
{
    public struct BitMask
    {
        public static bool ContainsLayer(int mask, int layer)
        {
            return (mask & layer) != 0;
        }

        public static bool ContainsLayer(LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }
    }
}