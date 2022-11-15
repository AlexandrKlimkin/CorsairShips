using UnityEngine;

namespace UnityDI.Providers 
{
    public class UnityScriptableObjectProviderLazyOrNot<T> : IObjectProvider<T> where T : ScriptableObject 
    {
        private T _instance;

        private string _fullPath;

        public UnityScriptableObjectProviderLazyOrNot(bool lazyInit = false, string fullPath = null) {
            _fullPath = fullPath;

            if (!lazyInit)
            {
                _instance = CreateInstance();
            }
        }

        public T GetObject(Container container)
        {
            if (_instance) 
            {
                return _instance;
            }
            return _instance = CreateInstance();
        }

        private T CreateInstance()
        {
            T instance;

            if (_fullPath != null) 
            {
                var obj = Resources.Load(_fullPath);
                if (obj != null)
                {
                    instance = (T) obj;
                    return instance;
                }
                Debug.LogWarning($"Cant load config from this path. ScriptableObject instance type of {typeof(T)} is creating.");
            }
            instance = ScriptableObject.CreateInstance<T>();
            SetSingletonName(instance);
            return instance;
        }

        private static void SetSingletonName(ScriptableObject singleton)
        {
            singleton.name = typeof(T).ToString();
        }
    }
}
