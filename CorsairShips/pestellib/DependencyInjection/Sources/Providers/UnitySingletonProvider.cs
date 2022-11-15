using UnityEngine;

namespace UnityDI.Providers
{
    public class UnitySingletonProvider<T> : IObjectProvider<T> where T : Component
    {
        private T _instance;

        private bool _inited;

        private bool _persistent;

        public UnitySingletonProvider(bool persistent)
        {
            _persistent = persistent;
        }

        public T GetObject(Container container)
        {
            /*
            if (applicationIsQuitting)
            {
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                    "' already destroyed on application quit." +
                    " Won't create again - returning null.");
                return null;
            }
            */

            if (_instance == null)
            {
                var instances = Object.FindObjectsOfType(typeof (T));

                if (instances.Length> 1)
                {
                    Debug.LogError("[Singleton] Something went really wrong " +
                        " - there should never be more than 1 singleton with type " + typeof(T).Name +
                        " Reopenning the scene might fix it.");
                }

                if (instances.Length > 0)
                {
                    _instance = (T)instances[0];
                    return _instance;
                }

                Object prefab = Resources.Load("Singleton/" + typeof (T).Name);

                if (prefab != null)
                {
                    GameObject singletonGameObject = (GameObject)Object.Instantiate(prefab);
                    if (singletonGameObject != null)
                    {
                        SetSingletonName(singletonGameObject);
                        _instance = singletonGameObject.GetComponent(typeof(T)) as T;
                        TrySetPersistent();
                        return _instance;
                    }
                }

                if (_instance == null)
                {
                    GameObject singleton = new GameObject();
                    _instance = singleton.AddComponent<T>();
                    SetSingletonName(singleton);

                    //Debug.Log("[Singleton] An instance of " + typeof(T) +
                    //    " is needed in the scene, so '" + singleton +
                    //    "' was created.");
                }
                else
                {
                    //Debug.Log("[Singleton] Using instance already created: " +
                    //    _instance.gameObject.name);
                }
            }

            if (!_inited)
            {
                _inited = true;
                container.BuildUp(_instance);
            }

            TrySetPersistent();
            return _instance;
        }

        private void TrySetPersistent()
        {
            if (_persistent)
            {
                GameObject.DontDestroyOnLoad(_instance.gameObject);
            }
        }

        private static void SetSingletonName(GameObject singleton)
        {
            singleton.name = typeof (T).ToString();
        }
    }
}
