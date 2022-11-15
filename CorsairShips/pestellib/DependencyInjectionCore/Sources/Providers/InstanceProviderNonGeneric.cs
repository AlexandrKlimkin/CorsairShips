using System;

namespace UnityDI.Providers
{
    public class InstanceProviderNonGeneric : IProviderWrapper
    {
        private readonly object _instance;

        public InstanceProviderNonGeneric(object instance)
        {
            _instance = instance;
        }

        public object GetObject(Container container)
        {
            return _instance;
        }
    }
}