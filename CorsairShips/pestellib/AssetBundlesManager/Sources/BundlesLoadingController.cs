using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PestelLib.Utils;

namespace PestelLib.AssetBundles
{
    public class BundlesLoadingController : MonoBehaviour
    {
        public const string SHIP_BUNDLES_GROUP = "ships";
        public static Action<string, bool> LoadingFinished = (x, success) => LogMessage(string.Format("finished loading bundle with a name: {0}, success: {1}", x, success));

        private static TypedLogger<BundlesLoadingController> _logger = new TypedLogger<BundlesLoadingController>();
        private List<string> _downloadList = new List<string>();
        private List<string> _failedList = new List<string>();
        private Coroutine _loadingCoroutine;
        private static BundlesLoadingController _instance;

        private bool _wasBundlesListInitialized = false;
        private AssetBundleManager _assetBundleManager;

        public static BundlesLoadingController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<BundlesLoadingController>();

                return _instance;
            }
        }

        public void Init(AssetBundleManager assetBundleManager)
        {
            _assetBundleManager = assetBundleManager;
            StartCoroutine(LoadBundlesTimer());
        }

        public void AddDownload(string bundleName)
        {
            if (_failedList.Contains(bundleName))
                _failedList.Remove(bundleName);

            if (_downloadList.Contains(bundleName))
            {
                LogMessage(string.Format("bundle {0} moved to the top of a download list", bundleName));
                _downloadList.Remove(bundleName);
            }
            else
                LogMessage(string.Format("added to download list a bundle with a name: {0}", bundleName));

            _downloadList.Add(bundleName);

            if (_loadingCoroutine == null)
                _loadingCoroutine = StartCoroutine(LoadBundles());
        }


        private const int BUNDLES_LOADING_PERIOD = 60;
        private IEnumerator LoadBundlesTimer()
        {
            while (true)
            {
                GenerateBundlesList();

                if (_loadingCoroutine == null)
                    _loadingCoroutine = StartCoroutine(LoadBundles());

                yield return new WaitForSeconds(BUNDLES_LOADING_PERIOD);
            }
        }

        private void GenerateBundlesList()
        {
            if (!_assetBundleManager.IsInitialized)
                return;

            if (!_wasBundlesListInitialized)
            {
                _wasBundlesListInitialized = true;

                var bundleNames = _assetBundleManager.Manifest.GetAllAssetBundles();
                foreach (var item in bundleNames)
                {
                    if (item.Contains(SHIP_BUNDLES_GROUP))
                        _downloadList.Add(item);
                }
            }

            _downloadList.AddRange(_failedList);
            _failedList.Clear();
        }

        private IEnumerator LoadBundles()
        {
            while (_downloadList.Count > 0)
            {
                var bundleName = _downloadList[_downloadList.Count - 1];
                var success = true;
                if (!_assetBundleManager.IsBundleCached(bundleName))
                    if (_assetBundleManager.IsBundleLoadingAllowed())
                    {
                        var handle = _assetBundleManager.DownloadBundle(bundleName);
                        while (!handle.isDone)
                            yield return null;
                        if (!handle.success)
                            _failedList.Add(bundleName);
                        success = handle.success;
                    }
                    else
                    {
                        LogMessage(string.Format("bundle with a name {0} won't be loaded in battle!", bundleName));
                        _failedList.Add(bundleName);
                        success = false;
                    }
                else
                    LogMessage(string.Format("bundle with a name {0} was already cached!", bundleName));

                LoadingFinished(bundleName, success);
                _downloadList.Remove(bundleName);
            }

            _loadingCoroutine = null;
        }

        private static void LogMessage(string msg)
        {
#if DEBUG_PROGRESS
            _logger.Log(msg);
#endif
        }
    }
}
