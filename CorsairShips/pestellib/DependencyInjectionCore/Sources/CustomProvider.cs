using System;

namespace UnityDI.Providers
{
    namespace UnityDI.Providers
    {
        public class CustomProvider<T> : IObjectProvider<T> where T : class
        {
            private readonly Func<T> _getter;

            public CustomProvider(Func<T> providerFunc)
            {
                _getter = providerFunc;
            }

            public T GetObject(Container container)
            {
                return _getter();
            }
        }
    }
}
