using System;

namespace UnityDI.Providers
{
    namespace UnityDI.Providers
    {
        public class CustomProviderSingleton<T> : IObjectProvider<T> where T : class
        {
            private readonly Func<T> _getter;
            private T _instance;

            public CustomProviderSingleton(Func<T> providerFunc)
            {
                _getter = providerFunc;
            }

            public T GetObject(Container container)
            {
                if (_instance == null)
                {
                    _instance = _getter();
                }

                return _instance;
            }
        }
    }
}
