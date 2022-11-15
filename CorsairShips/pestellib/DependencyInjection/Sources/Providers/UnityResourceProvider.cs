using UnityDI.Providers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityDI.Sources.Providers
{
    public class UnityResourceProvider<T> : IObjectProvider<T> where T : Object
    {
        private string _path;

        public UnityResourceProvider(string path)
        {
            _path = path;
        }

        public T GetObject(Container container)
        {
            var loaded = Resources.Load(_path);

            var instance = Object.Instantiate(loaded);

            if (typeof(T) == typeof(GameObject))
            {
                return (T)instance; //GameObject
            }

            var go = instance as GameObject;
            if (go != null)
            {
                return go.GetComponent<T>(); //GameObject component
            }

            return (T)instance; //Material, AudioClip etc.
        }
    }
}
