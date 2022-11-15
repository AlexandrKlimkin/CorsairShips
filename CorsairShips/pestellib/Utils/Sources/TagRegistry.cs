using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PestelLib.Utils
{
    public static class TagRegistry
    {
        private static Dictionary<string, HashSet<Tag>> _registry = new Dictionary<string, HashSet<Tag>>();

        public static void AddObject(Tag obj)
        {
            for (var i = 0; i < obj.Tags.Length; i++)
            {
                var key = obj.Tags[i];
                if (_registry.ContainsKey(key))
                {
                    if (!_registry[key].Contains(obj))
                    {
                        _registry[key].Add(obj);
                    }
                }
                else
                {
                    _registry[key] = new HashSet<Tag> { obj };
                }
            }
        }

        public static void RemoveObject(Tag obj)
        {
            for (var i = 0; i < obj.Tags.Length; i++)
            {
                var key = obj.Tags[i];
                if (_registry.ContainsKey(key))
                {
                    _registry[key].Remove(obj);
                }
            }
        }

        private static bool IsPrefab(Tag tag) {
            return tag.hideFlags != HideFlags.None;
        }

        public static GameObject GetObjectByTag(string t)
        {
            if (_registry.ContainsKey(t))
            {
                var obj = _registry[t].FirstOrDefault();
                if (obj != null)
                {
                    return obj.gameObject;
                }
            }
            Debug.LogWarning(string.Format("Object with custom tag: {0} not found", t));
            return null;
        }

        public static T GetObjectByTag<T>(string t) where T : class
        {
            if (!_registry.ContainsKey(t))
            {
                Debug.LogWarning("Can't find object with custom tag = " + t);
                return null;
            }

            var obj = _registry[t].FirstOrDefault();
            if (obj != null)
            {
                return obj.gameObject.GetComponent<T>();
            }
            else
            {
                Debug.LogWarning(string.Format("Object with custom tag: {0} not found", t));
                return null;
            }
        }

        //don't modify returned collection!
        public static HashSet<Tag> GetObjectsByTag(string t)
        {
            if (_registry.ContainsKey(t))
            {
                return _registry[t];
            }
            else
            {
                Debug.LogWarning(string.Format("Objects with custom tag: {0} not found", t));
                return null;
            }
        }
    }
}