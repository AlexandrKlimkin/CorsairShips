using UnityEngine;

namespace UnityDI.Providers {
    public class UnitySingletonProviderLazyOrNot<T> : IObjectProvider<T> where T : Component 
    {
        private T _instance;

        private bool _persistent;

        private string _fullPath;

        public UnitySingletonProviderLazyOrNot(bool persistent = false, bool lazyInit = false, string fullPath = null) 
        {
            _persistent = persistent;
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
            GameObject singleton = null;

            if (_fullPath != null)
            {
                var prefab = Resources.Load(_fullPath);
                if (prefab != null) 
                {
                    singleton = (GameObject) Object.Instantiate(prefab);
                    instance = singleton.GetComponent(typeof(T)) as T;
                    if (instance != null) 
                    {
                        if (_persistent)
                        {
                            Object.DontDestroyOnLoad(instance.gameObject);
                        }
                        return instance;
                    }
                }
                Debug.LogWarning($"Cant load prefab from this path. New GameObject with component {typeof(T)} is creating.");
            }
            singleton = new GameObject();
            instance = singleton.AddComponent<T>();           
            singleton.name = typeof(T).ToString();
            if (_persistent)
            {
                Object.DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }
}