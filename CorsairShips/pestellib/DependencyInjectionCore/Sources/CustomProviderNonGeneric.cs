using System;

namespace UnityDI.Providers
{
    namespace UnityDI.Providers
    {
        public class CustomProviderNonGeneric : IProviderWrapper
        {
            private readonly Func<object> _getter;

            public CustomProviderNonGeneric(Type type, Func<object> providerFunc)
            {
                _getter = providerFunc;
            }

            public object GetObject(Container container)
            {
                return _getter();
            }
        }
    }
}
