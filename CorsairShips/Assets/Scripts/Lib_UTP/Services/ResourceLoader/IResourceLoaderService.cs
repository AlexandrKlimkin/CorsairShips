using System;
using UnityEngine;

namespace UTPLib.Services.ResourceLoader 
{
    public interface IResourceLoaderService 
    {
        T LoadResource<T>(string path, string name) where T : UnityEngine.Object;

        //T LoadResource<T>(ResourceAsset resourceAsset) where T : UnityEngine.Object;

        T LoadResource<T>(string fullPath) where T : UnityEngine.Object;

        T LoadResourceOnScene<T>(string fullPath, Transform parent = null, bool worldPositionStays = false)
            where T : UnityEngine.Object;

        T LoadResourceOnScene<T>(string fullPath, Vector3 position, Quaternion rotation, Transform parent = null)
            where T : UnityEngine.Object;

        void LoadResourceAsync<T>(string fullPath, Action<T> callback) where T : UnityEngine.Object;

        void LoadResourceAsync<T>(string path, string name, Action<T> callback) where T : UnityEngine.Object;

        //void LoadResourceAsync<T>(ResourceAsset resourceAsset, Action<T> callback) where T : UnityEngine.Object;

        void LoadResourceOnSceneAsync<T>(string fullPath, Action<T> callback, Transform parent = null,
            bool worldPositionStays = false) where T : UnityEngine.Object;

        void LoadResourceOnSceneAsync<T>(string fullPath, Action<T> callback, Vector3 position, Quaternion rotation,
            Transform parent = null) where T : UnityEngine.Object;

        T[] LoadAllResources<T>(string path) where T : UnityEngine.Object;
    }
}
