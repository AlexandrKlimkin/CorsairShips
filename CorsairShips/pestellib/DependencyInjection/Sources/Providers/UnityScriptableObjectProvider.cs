using UnityEngine;

namespace UnityDI.Providers
{
    public class UnityScriptableObjectProvider<T> : IObjectProvider<T> where T : ScriptableObject
    {
        private T _instance;

        private bool _inited;

        public UnityScriptableObjectProvider()
        {
        }

        public T GetObject(Container container)
        {
            if (_instance == null)
            {
                var obj = Resources.Load("Singleton/" + typeof (T).Name);

                if (obj != null)
                {
                    _instance = (T)Object.Instantiate(obj);
                } else {
                    _instance = ScriptableObject.CreateInstance<T>();
                    SetSingletonName(_instance);
                }
            }

            if (!_inited)
            {
                _inited = true;
                container.BuildUp(_instance);
            }

            return _instance;
        }

        private static void SetSingletonName(ScriptableObject singleton)
        {
            singleton.name = typeof(T).ToString();
        }
    }
}