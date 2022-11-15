using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;
using BestHTTP;

namespace PestelLib.AssetBundles
{
    public class DownloadHandle
    {
        public bool isDone;
        public bool success;
        public float progress;
        public HTTPRequest request;
    }

    public class AssetBundleManager : MonoBehaviour
    {
        public bool IsInitialized
        {
            get
            {
                return Manifest != null;
            }
            private set { }
        }

        [System.NonSerialized]
        public string BaseDownloadingURL;

        [System.NonSerialized]
        public AssetBundleManifest Manifest = null;
        public Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();
        [System.NonSerialized]
        public List<string> bundledScenes = new List<string>();

        Dictionary<string, HTTPRequest> _downloadRequests = new Dictionary<string, HTTPRequest>();

        public void SetSourceAssetBundleURL(string absolutePath)
        {
            if (!absolutePath.EndsWith("/"))
                absolutePath += "/";
            BaseDownloadingURL = absolutePath + Utility.GetPlatformName() + "/";
        }

        public IEnumerator Initialize()
        {
            while (true)
            {
                var handle = DownloadBundle(Utility.GetPlatformName());
                while (!handle.isDone)
                {
                    yield return null;
                }

                if (handle.request.Response == null || !handle.success && handle.request.Response != null && handle.request.Response.StatusCode != 404)
                {
                    yield return HandleError();

                    continue;
                } else if (handle.request.Response != null && handle.request.Response.StatusCode == 404) {
                    break;
                }

                var bundle = GetBundle(Utility.GetPlatformName());
                Manifest = bundle.LoadAsset("AssetBundleManifest") as AssetBundleManifest;
                var assetBundles = Manifest.GetAllAssetBundles();
                
                foreach (var bundleName in assetBundles)
                {
                    if (bundleName.StartsWith("scenes"))
                    {
                        bundledScenes.Add(bundleName);
                    }
                }
                break;
            }
        }

        protected virtual IEnumerator HandleError()
        {
            yield return null;
        }

        public DownloadHandle DownloadBundle(string assetBundleName)
        {
            var handle = new DownloadHandle();
            var uri = new System.Uri(BaseDownloadingURL + assetBundleName);
            var request = new HTTPRequest(uri, HTTPMethods.Get, false, true, (req, response) =>
            {
                handle.isDone = true;
                if (response != null && response.IsSuccess)
                {
                    var data = response.Data;
                    var bundle = AssetBundle.LoadFromMemory(data);
                    if (bundle != null)
                    {
                        Utility.SaveBytesToFile(data, assetBundleName);
                        handle.success = true;
                        loadedBundles[assetBundleName] = bundle;
                    }
                }
            });
            request.OnProgress += (requset, count, length) =>
            {
                handle.progress = count * 1f / length;
            };
            request.Send();
            handle.request = request;
            _downloadRequests[assetBundleName] = request;

            return handle;
        }

        public AssetBundle GetBundle(string assetBundleName)
        {
            if (loadedBundles.ContainsKey(assetBundleName))
            {
                return loadedBundles[assetBundleName];
            }

            var data = Utility.LoadBytes(assetBundleName);
            var bundle = AssetBundle.LoadFromMemory(data);
            loadedBundles[assetBundleName] = bundle;
            return bundle;
        }

        public bool IsBundleCached(string assetBundleName)
        {
            return Utility.Exists(assetBundleName);
        }

        public void CancelBundleDownload(string assetBundleName)
        {
            if (_downloadRequests.ContainsKey(assetBundleName))
            {
                _downloadRequests[assetBundleName].Abort();
                _downloadRequests.Remove(assetBundleName);
            }
        }

        public bool SceneBundlesDownloaded()
        {
            foreach (var sceneName in bundledScenes)
            {
                if (!IsBundleCached(sceneName))
                {
                    return false;
                }
            }

            return true;
        }

        public void LoadDownloadedScenes()
        {
            foreach (var sceneName in bundledScenes)
            {
                if (IsBundleCached(sceneName))
                {
                    var data = Utility.LoadBytes(sceneName);
                    AssetBundle.LoadFromMemory(data);
                }
            }
        }

        public int GetDownloadedMapsCode()
        {
            string temp = "";
            foreach (var sceneName in bundledScenes)
            {
                if (IsBundleCached(sceneName))
                {
                    temp += sceneName;
                }
            }
            return temp.GetHashCode();
        }

        public virtual bool IsBundleLoadingAllowed()
        {
            return true;
        }
    }
}