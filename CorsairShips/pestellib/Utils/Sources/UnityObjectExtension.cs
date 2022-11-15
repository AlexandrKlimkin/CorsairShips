using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PestelLib.Utils
{
    public static class UnityObjectExtension
    {
        /// <summary>
        /// Get specific interface without odd casts and 
        /// just to shorten notation like var iDamage = (IDamage)gameObject.GetComponent(typeof (IDamage));
        /// to var iDamage = gameObject.GetInterface<IDamage>();
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <returns></returns>
        public static T GetInterface<T>(this GameObject go) where T : class
        {
            return go.GetComponent(typeof (T)) as T;
        }

        public static T GetInterface<T>(this Transform t) where T : class
        {
            return t.GetComponent(typeof (T)) as T;
        }

        public static T GetInterface<T>(this MonoBehaviour mb) where T : class
        {
            return mb.GetComponent(typeof (T)) as T;
        }

        public static IEnumerable<T> GetInterfaces<T>(this GameObject go) where T : class
        {
            return go.GetComponents(typeof (T)).OfType<T>();
        }

        public static IEnumerable<T> GetInterfaces<T>(this Transform t) where T : class
        {
            return t.GetComponents(typeof (T)).OfType<T>();
        }

        public static IEnumerable<T> GetInterfaces<T>(this MonoBehaviour mb) where T : class
        {
            return mb.GetComponents(typeof (T)).OfType<T>();
        }
    }
}