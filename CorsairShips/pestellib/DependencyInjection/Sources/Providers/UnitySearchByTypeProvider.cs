using UnityEngine;

namespace UnityDI.Providers
{
    public class UnitySearchByTypeProvider<T> : IObjectProvider<T> where T : Component
    {
        public UnitySearchByTypeProvider()
        {
        }

        public T GetObject(Container container)
        {
            return Object.FindObjectOfType<T>();
        }
    }
}