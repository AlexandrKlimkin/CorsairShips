using UnityEngine;

namespace PestelLib.Utils
{
    public static class LayerUtils
    {
        public static bool IsIntersect(this LayerMask layermask, int layer)
        {
            return layermask == (layermask | (1 << layer));
        }

        public static void ChangeLayersRecursively(this Transform trans, int layer)
        {
            trans.gameObject.layer = layer;
            foreach (Transform child in trans)
            {
                child.ChangeLayersRecursively(layer);
            }
        }

        public static void ChangeLayersRecursively(this GameObject go, int layer)
        {
            ChangeLayersRecursively(go.transform, layer);
        }

        public static void ChangeLayersRecursively(this Transform trans, string layerName)
        {
            ChangeLayersRecursively(trans, LayerMask.NameToLayer(layerName));
        }

        public static void ChangeLayersRecursively(this GameObject go, string layerName)
        {
            ChangeLayersRecursively(go.transform, LayerMask.NameToLayer(layerName));
        }
    }
}
