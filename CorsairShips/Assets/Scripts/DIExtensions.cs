using System;
using UnityDI;

namespace PestelLib.SharedLogic.Extentions
{
    public static class DIExtensions
    {
        /// <summary>
        /// Unregisters given instance only if it was previously registred.
        /// </summary>
        public static void UnregisterInstance<T>(this Container container, T instance) where T : class
        {
            if (container.Resolve<T>() != instance)
                return;

            container.Unregister<T>();
        }

        /// <summary>
        /// Unregisters given instance only if it was previously registred.
        /// </summary>
        public static void UnregisterInstance(this Container container, Type type, object instance)
        {
            if (container.Resolve(type) != instance)
                return;

            container.Unregister(type);
        }
    }
}
