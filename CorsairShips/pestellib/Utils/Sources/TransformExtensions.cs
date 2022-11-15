using System;
using System.Collections.Generic;
using UnityEngine;

namespace PestelLib.Utils
{
    public static class ExtensionMethods
    {
        public static Transform FindInChildren(this Transform self, string name)
        {
            int count = self.childCount;
            for (int i = 0; i < count; i++)
            {
                Transform child = self.GetChild(i);
                if (child.name == name) return child;
                Transform subChild = child.FindInChildren(name);
                if (subChild != null) return subChild;
            }
            return null;
        }

        public static GameObject FindInChildren(this GameObject self, string name)
        {
            Transform transform = self.transform;
            Transform child = transform.FindInChildren(name);
            return child != null ? child.gameObject : null;
        }

        public static void CopyFromRectTransform(this RectTransform self, RectTransform source)
        {
            self.anchoredPosition = source.anchoredPosition;
            self.anchoredPosition3D = source.anchoredPosition3D;
            self.anchorMax = source.anchorMax;
            self.anchorMin = source.anchorMin;
            self.offsetMax = source.offsetMax;
            self.offsetMin = source.offsetMin;
            self.pivot = source.pivot;
            self.sizeDelta = source.sizeDelta;
            self.parent = source.parent;
        }

        public static string FullPath(this Transform self)
        {
            var path = new List<string>();
            var current = self;
            do
            {
                path.Insert(0, current.name);
                current = current.parent;
            } while (current != null);

            return string.Join("/", path.ToArray());
        }
    }
}