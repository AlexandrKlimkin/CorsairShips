using System;
using UnityDI.Providers;
using UnityDI.Providers.UnityDI.Providers;
using UnityDI.Sources.Providers;
using UnityEngine;

namespace UnityDI
{
    public static class ContainerExtensions
    {
        /// <summary>
        /// Зарегистрировать путь в дереве сцены
        /// При каждом обращении Resolve&lt;T&gt;() будет объект типа T, найденный по пути в дереве сцены
        /// Путь может указывать на неактивный объект, однако должен начинаться с активного объекта!
        /// В пути может встречаться символ '*', что обозначает первый активный объект
        /// </summary>
        public static Container RegisterSceneObject<T>(this Container container, string path, string name = null) where T : class
        {
            return container.RegisterProvider<T>(new UnityScenePathProvider<T>(path), name);
        }

        public static Container RegisterUnityScriptableObject<T>(this Container container) where T : ScriptableObject
        {
            return container.RegisterProvider(new UnityScriptableObjectProvider<T>());
        }

        public static Container RegisterUnityScriptableObject<T>(this Container container, string name) where T : ScriptableObject
        {
            return container.RegisterProvider(new UnityScriptableObjectProvider<T>(), name);
        }

        public static Container RegisterUnityScriptableObjectLazyOrNot<T>(this Container container, bool lazyInit = false, string fullPath = null) where T : ScriptableObject {
            return container.RegisterProvider(new UnityScriptableObjectProviderLazyOrNot<T>(lazyInit, fullPath));
        }

        public static Container RegisterUnityScriptableObjectLazyOrNot<T>(this Container container, string name, bool lazyInit = false, string fullPath = null) where T : ScriptableObject {
            return container.RegisterProvider(new UnityScriptableObjectProviderLazyOrNot<T>(lazyInit, fullPath), name);
        }

        public static Container RegisterUnitySingleton<T>(this Container container, string name = null, bool persistent = false) where T : Component
        {
            return container.RegisterProvider(new UnitySingletonProvider<T>(persistent), name);
        }

        public static Container RegisterUnitySingleton<TBase, TDerived>(this Container container, string name = null, bool persistent = false)
            where TDerived : Component, TBase
        {
            return container.RegisterProvider<TBase, TDerived>(new UnitySingletonProvider<TDerived>(persistent), name);
        }

        public static Container RegisterUnitySingletonLazyOrNot<T>(this Container container, string name = null, bool persistent = false, bool lazyInit = false, string fullPath = null) where T : Component 
        {
            return container.RegisterProvider(new UnitySingletonProviderLazyOrNot<T>(persistent, lazyInit, fullPath), name);
        }

        public static Container RegisterUnitySingletonLazyOrNot<TBase, TDerived>(this Container container, string name = null, bool persistent = false, bool lazyInit = false, string fullPath = null)
            where TDerived : Component, TBase {
            return container.RegisterProvider<TBase, TDerived>(new UnitySingletonProviderLazyOrNot<TDerived>(persistent, lazyInit, fullPath), name);
        }

        public static Container RegisterUnityTag<T>(this Container container, string tag = null, string name = null)
            where T : Component
        {
            return container.RegisterProvider(new UnitySearchByTagProvider<T>(tag), name);
        }

        public static Container RegisterUnityTag<TBase, TDerived>(this Container container, string tag = null, string name = null)
            where TDerived : Component, TBase
        {
            return container.RegisterProvider<TBase, TDerived>(new UnitySearchByTagProvider<TDerived>(tag), name);
        }

        public static Container RegisterUnityType<T>(this Container container, string name = null)
            where T : Component
        {
            return container.RegisterProvider(new UnitySearchByTypeProvider<T>(), name);
        }

        public static Container RegisterUnityType<TBase, TDerived>(this Container container, string name = null)
            where TDerived : Component, TBase
        {
            return container.RegisterProvider<TBase, TDerived>(new UnitySearchByTypeProvider<TDerived>(), name);
        }

        public static Container RegisterUnityResource<T>(this Container container, string resourcePath, string name = null)
            where T : Component
        {
            return container.RegisterProvider(new UnityResourceProvider<T>(resourcePath), name);
        }

        public static Container RegisterUnityResource<TBase, TDerived>(this Container container, string resourcePath, string name = null) 
            where TDerived : Component, TBase
        {
            return container.RegisterProvider<TBase, TDerived>(new UnityResourceProvider<TDerived>(resourcePath), name);
        }
    }
}